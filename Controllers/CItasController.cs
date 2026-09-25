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

        // 1. Endpoint Tarea 1 (Backend support): Registrar llegada de cliente sin cita
[HttpPost("registrar-sin-cita")]
public async Task<IActionResult> RegistrarClienteSinCita([FromBody] RegistrarSinCitaDto dto)
{
    var nuevaCita = new Cita
    {
        IdCliente = dto.IdCliente,
        IdBarbero = dto.IdBarbero,
        IdServicio = dto.IdServicio,
        FechaHora = DateTime.UtcNow, // Hora actual de llegada
        Estado = "En Espera",
        DuracionTotalMin = dto.DuracionMinutos,
        EsSinCita = true // Marca que no tenía reserva previa
    };

    _context.Citas.Add(nuevaCita);
    await _context.SaveChangesAsync();

    return Ok(new 
    { 
        mensaje = "Cliente registrado en la cola de espera exitosamente.",
        citaId = nuevaCita.Id 
    });
}

// 2. Endpoint Tarea 2: Obtener Cola de Espera Priorizada (RF-12)
[HttpGet("cola-espera/{idBarbero}")]
public async Task<IActionResult> ObtenerColaEsperaPriorizada(int idBarbero)
{
    // Usamos DateTime.UtcNow.Date para evitar conflictos de zona horaria con PostgreSQL
    var fechaHoyUtc = DateTime.UtcNow.Date;

    var colaPriorizada = await _context.Citas
        .Where(c => c.IdBarbero == idBarbero 
                 && c.FechaHora.Date == fechaHoyUtc 
                 && (c.Estado == "Agendada" || c.Estado == "En Espera"))
        .OrderBy(c => c.EsSinCita) // False (Reserva) va PRIMERO, True (Sin Cita) va DESPUÉS
        .ThenBy(c => c.FechaHora)
        .Select(c => new 
        {
            c.Id,
            c.IdCliente,
            c.IdBarbero,
            c.IdServicio,
            Hora = c.FechaHora.ToString("HH:mm"),
            Tipo = c.EsSinCita ? "Sin Cita (Walk-in)" : "Reserva Previa",
            Prioridad = c.EsSinCita ? 2 : 1,
            c.Estado
        })
        .ToListAsync();

    return Ok(colaPriorizada);
}

public class RegistrarSinCitaDto
    {
        public int IdCliente { get; set; }
        public int IdBarbero { get; set; }
        public int IdServicio { get; set; }
        public int DuracionMinutos { get; set; } = 40;
    }
    }
}