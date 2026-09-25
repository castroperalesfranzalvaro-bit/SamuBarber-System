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
    }
}