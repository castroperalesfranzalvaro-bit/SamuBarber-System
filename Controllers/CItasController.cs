using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("agendar")]
        public async Task<IActionResult> AgendarCita([FromBody] AgendarCitaDto dto)
        {
            var servicio = await _context.Servicios.FindAsync(dto.IdServicio);
            if (servicio == null)
            {
                return BadRequest(new { mensaje = "El servicio seleccionado no existe." });
            }

            int duracionBloque = servicio.DuracionMinutos > 0 ? servicio.DuracionMinutos : 40;

            DateTime inicioNuevaCita = dto.FechaHora;
            DateTime finNuevaCita = inicioNuevaCita.AddMinutes(duracionBloque);

            // Validación de cruce de horarios (Tarea 3)
            bool existeCruce = await _context.Citas.AnyAsync(c =>
                c.IdBarbero == dto.IdBarbero &&
                c.Estado == "Agendada" &&
                ((inicioNuevaCita >= c.FechaHora && inicioNuevaCita < c.FechaHora.AddMinutes(c.DuracionTotalMin)) ||
                 (finNuevaCita > c.FechaHora && finNuevaCita <= c.FechaHora.AddMinutes(c.DuracionTotalMin)) ||
                 (inicioNuevaCita <= c.FechaHora && finNuevaCita >= c.FechaHora.AddMinutes(c.DuracionTotalMin)))
            );

            if (existeCruce)
            {
                return Conflict(new { mensaje = "El barbero seleccionado ya tiene una cita reservada en ese horario." });
            }

            var nuevaCita = new Cita
            {
                IdCliente = dto.IdCliente,
                IdBarbero = dto.IdBarbero,
                IdServicio = dto.IdServicio,
                FechaHora = dto.FechaHora,
                Estado = "Agendada",
                DuracionTotalMin = duracionBloque
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

            return Ok(new { 
                mensaje = "Cita agendada exitosamente.", 
                idCita = nuevaCita.IdCita,
                confirmacion = $"Reserva confirmada para el {inicioNuevaCita:dd/MM/yyyy a las HH:mm} hs."
            });
        }
    }
}