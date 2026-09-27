using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;
using System.Globalization;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TAREA 1: Consultas de agregación SQL para ingresos, egresos y utilidades mensuales
        // TAREA 3: Control de acceso / Seguridad por Rol (Denegar a Barbero)
        [HttpGet("mensual")]
        public async Task<IActionResult> ObtenerReporteMensual(
            [FromQuery] int? anio = null, 
            [FromHeader(Name = "X-Rol-Usuario")] string? rolUsuario = "Administrador")
        {
            // TAREA 3: Seguridad - Denegar acceso si el usuario es "Barbero"
            if (string.Equals(rolUsuario, "Barbero", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { mensaje = "Acceso Denegado: Los barberos solo pueden ver sus comisiones individuales y no los totales globales de la caja." });
            }

            int anioConsulta = anio ?? DateTime.UtcNow.Year;

            // Agregación de ventas agrupadas por mes
            var ventasDelAnio = await _context.Ventas
                .Where(v => v.FechaVenta.Year == anioConsulta)
                .ToListAsync();

            var reportes = ventasDelAnio
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g =>
                {
                    decimal ingresosBrutos = g.Sum(v => v.Total);
                    decimal comisionesBarberos = ingresosBrutos * 0.50m; // 50% para barberos
                    decimal reservaOperativa = ingresosBrutos * 0.20m;   // 20% reserva operativa
                    decimal totalEgresos = comisionesBarberos + reservaOperativa;
                    decimal utilidadNeta = ingresosBrutos - totalEgresos;

                    return new ReporteMensualDto
                    {
                        Anio = anioConsulta,
                        MesNumero = g.Key,
                        MesNombre = DateTimeFormatInfo.CurrentInfo.GetMonthName(g.Key).ToUpper(),
                        IngresosBrutos = ingresosBrutos,
                        EgresosComisionesBarberos = comisionesBarberos,
                        EgresosReservaOperativa = reservaOperativa,
                        TotalEgresos = totalEgresos,
                        UtilidadNeta = utilidadNeta,
                        TotalVentasRealizadas = g.Count()
                    };
                })
                .OrderBy(r => r.MesNumero)
                .ToList();

            return Ok(reportes);
        }
    }
}