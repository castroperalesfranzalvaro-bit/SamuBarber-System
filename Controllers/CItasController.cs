using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;
using SamuBarber.Api.Models;
using SamuBarber.Api.Services;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditoriaService _auditoria;

        public CitasController(ApplicationDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // --- ENDPOINT PARA CANCELAR CITA (Tarea 2) ---
        [HttpPost("cancelar")]
public async Task<IActionResult> CancelarCita([FromBody] CancelarCitaDto dto)
{
    var cita = await _context.Citas.FirstOrDefaultAsync(c => c.Id == dto.IdCita);

    if (cita == null)
    {
        return NotFound(new { mensaje = "La cita no existe." });
    }

    cita.Estado = "Cancelada";
    cita.MotivoCancelacion = dto.Motivo;
    cita.FechaModificacion = DateTime.UtcNow;
    cita.UsuarioModificacion = dto.UsuarioResponsable;

    await _context.SaveChangesAsync();

    return Ok(new { mensaje = "Cita cancelada con éxito." });
}

        // --- ENDPOINT PARA REPROGRAMAR CITA (RF-07) ---
        [HttpPut("reprogramar")]
        public async Task<IActionResult> ReprogramarCita([FromBody] ReprogramarCitaDto dto)
        {
            var cita = await _context.Citas.FindAsync(dto.IdCita);

            if (cita == null)
            {
                return NotFound(new { mensaje = "La cita especificada no existe." });
            }

            int barberoId = dto.NuevoIdBarbero ?? cita.IdBarbero;

            // Validar que el nuevo horario no choque con otra cita activa del barbero
            bool existeSolapamiento = await _context.Citas.AnyAsync(c =>
                c.Id != dto.IdCita &&
                c.IdBarbero == barberoId &&
                c.Estado != "Cancelada" &&
                c.FechaHora == dto.NuevaFechaHora);

            if (existeSolapamiento)
            {
                return Conflict(new { mensaje = "El barbero no tiene disponibilidad en la nueva fecha u hora seleccionada." });
            }

            DateTime fechaAnterior = cita.FechaHora;

            // Actualizar cita
            cita.FechaHora = dto.NuevaFechaHora;
            cita.IdBarbero = barberoId;
            cita.Estado = "Reprogramada";
            cita.FechaModificacion = DateTime.Now;
            cita.UsuarioModificacion = dto.UsuarioResponsable;

            await _context.SaveChangesAsync();

            // Registrar Auditoría (Tarea 3 - RNF-20)
            _auditoria.RegistrarAnulacionOModificacion(
                cita.Id,
                "REPROGRAMACIÓN",
                dto.UsuarioResponsable,
                $"Fecha anterior: {fechaAnterior:yyyy-MM-dd HH:mm} -> Nueva fecha: {dto.NuevaFechaHora:yyyy-MM-dd HH:mm}"
            );

            return Ok(new
            {
                mensaje = "Cita reprogramada exitosamente.",
                idCita = cita.Id,
                nuevaFechaHora = cita.FechaHora,
                idBarbero = cita.IdBarbero
            });
        }

   [HttpPost("registrar-sin-cita")]
public async Task<IActionResult> RegistrarClienteSinCita([FromBody] RegistrarSinCitaDto dto)
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.NombreCliente))
        {
            return BadRequest(new { mensaje = "El nombre del cliente es obligatorio." });
        }

        // 1. Buscar cliente por nombre
var cliente = await _context.Clientes
    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == dto.NombreCliente.Trim().ToLower());

// 2. Si no existe, crearlo asignando explícitamente el teléfono
if (cliente == null)
{
    cliente = new Cliente
    {
        Nombre = dto.NombreCliente.Trim(),
        Telefono = "0" // <-- ASIGNACIÓN EXPLÍCITA
    };
    
    _context.Clientes.Add(cliente);
    await _context.SaveChangesAsync();
}

        // 3. Registrar la cita asignando el ID del cliente guardado
        var nuevaCita = new Cita
        {
            IdCliente = cliente.IdCliente,
            IdBarbero = dto.IdBarbero,
            IdServicio = dto.IdServicio,
            FechaHora = DateTime.UtcNow,
            Estado = "En Espera",
            EsSinCita = true
        };

        _context.Citas.Add(nuevaCita);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            mensaje = "Cliente registrado en la cola de espera exitosamente.", 
            citaId = nuevaCita.Id 
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error interno en el servidor", detalle = ex.InnerException?.Message ?? ex.Message });
    }
}

// 2. Endpoint Tarea 2: Obtener Cola de Espera Priorizada (RF-12)
[HttpGet("cola-espera/{idBarbero}")]
public async Task<IActionResult> ObtenerColaEsperaPriorizada(int idBarbero)
{
    var fechaHoyUtc = DateTime.UtcNow.Date;

    // Hacemos el JOIN haciendo match con cl.IdCliente
    var colaPriorizada = await (from c in _context.Citas
                                join cl in _context.Clientes on c.IdCliente equals cl.IdCliente into clienteGroup
                                from cl in clienteGroup.DefaultIfEmpty()
                                where c.IdBarbero == idBarbero 
                                   && c.FechaHora.Date == fechaHoyUtc 
                                   && (c.Estado == "Agendada" || c.Estado == "En Espera")
                                orderby c.EsSinCita, c.FechaHora
                                select new
                                {
                                    c.Id,
                                    c.IdCliente,
                                    NombreCliente = cl != null ? cl.Nombre : "Cliente Presencial",
                                    c.IdBarbero,
                                    c.IdServicio,
                                    Hora = c.FechaHora.ToString("HH:mm"),
                                    Tipo = c.EsSinCita ? "Sin Cita (Walk-in)" : "Reserva Previa",
                                    Prioridad = c.EsSinCita ? 2 : 1,
                                    c.Estado
                                }).ToListAsync();

    return Ok(colaPriorizada);
}

[HttpPost("completar-y-cobrar")]
public async Task<IActionResult> CompletarYCobrar([FromBody] CobrarCitaDto dto)
{
    // 1. Buscar la cita
    var cita = await _context.Citas.FindAsync(dto.IdCita);
    if (cita == null) return NotFound(new { mensaje = "Cita no encontrada." });

    // 2. Marcar la cita como Completada
    cita.Estado = "Completada";

    // 3. Registrar la venta en la base de datos (o la lógica de caja)
    // Aquí puedes enlazar tu modelo de Ventas si ya lo tienes definido
    
    await _context.SaveChangesAsync();

    return Ok(new { mensaje = "Cobro procesado y cita completada con éxito." });
}

[HttpPut("completar/{id}")]
public async Task<IActionResult> CompletarCita(int id)
{
    var cita = await _context.Citas.FindAsync(id);
    if (cita == null)
        return NotFound("No se encontró la cita.");

    cita.Estado = "Completada"; // O el nombre de estado que use tu tabla (ej. "Atendido")
    await _context.SaveChangesAsync();

    return Ok(new { mensaje = "Cita completada exitosamente." });
}
public class RegistrarSinCitaDto
{
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = "00000000"; // <-- Agregar esta línea
    public int IdCliente { get; set; }
    public int IdBarbero { get; set; }
    public int IdServicio { get; set; }
    public int DuracionMinutos { get; set; } = 40;
}
public class CobrarCitaDto
{
    public int IdCita { get; set; }
    public decimal MontoTotal { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
}
    }
}