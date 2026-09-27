using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;
using System.Diagnostics;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CajaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TAREA 1 & 2: Endpoint para calcular el Cierre de Caja Diario y aplicar la Reserva Operativa
        [HttpGet("cierre-diario")]
        public async Task<IActionResult> ObtenerCierreDiario(
            [FromQuery] string tipoReserva = "porcentaje", // "porcentaje" o "fijo"
            [FromQuery] decimal valorReserva = 20m,       // Valor por defecto: 20%
            [FromQuery] DateTime? fecha = null)
        {
            // TAREA 3: Medición de rendimiento
            var stopwatch = Stopwatch.StartNew();

            var fechaConsulta = (fecha ?? DateTime.UtcNow).Date;
            var fechaFin = fechaConsulta.AddDays(1);

            // Obtener ventas del día seleccionado
            var ventasHoy = await _context.Ventas
                .Where(v => v.FechaVenta >= fechaConsulta && v.FechaVenta < fechaFin)
                .ToListAsync();

            // TAREA 1: Consolidar ingresos por método de pago (Efectivo y QR)
            decimal totalEfectivo = ventasHoy
                .Where(v => v.MetodoPago.Trim().Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
                .Sum(v => v.Total);

            decimal totalQR = ventasHoy
                .Where(v => v.MetodoPago.Trim().Equals("QR", StringComparison.OrdinalIgnoreCase) || 
                            v.MetodoPago.Trim().Contains("QR", StringComparison.OrdinalIgnoreCase))
                .Sum(v => v.Total);

            // Otros métodos si existieran
            decimal otrosMetodos = ventasHoy
                .Where(v => !v.MetodoPago.Trim().Equals("Efectivo", StringComparison.OrdinalIgnoreCase) && 
                            !v.MetodoPago.Trim().Contains("QR", StringComparison.OrdinalIgnoreCase))
                .Sum(v => v.Total);

            decimal ingresosTotales = totalEfectivo + totalQR + otrosMetodos;

            // TAREA 2: Algoritmo para calcular la reserva operativa (Alquiler, navajas, insumos)
            decimal montoReserva = 0m;
            if (tipoReserva.ToLower() == "fijo")
            {
                montoReserva = valorReserva;
            }
            else // "porcentaje"
            {
                montoReserva = ingresosTotales * (valorReserva / 100m);
            }

            // Asegurar que la reserva no supere el ingreso total
            montoReserva = Math.Min(montoReserva, ingresosTotales);

            // Calcular Utilidad Neta
            decimal utilidadNeta = ingresosTotales - montoReserva;

            stopwatch.Stop();
            double tiempoProcesamientoMs = stopwatch.Elapsed.TotalMilliseconds;

            var resumen = new ResumenCierreCajaDto
            {
                Fecha = fechaConsulta.ToString("yyyy-MM-dd"),
                TotalVentasCount = ventasHoy.Count,
                TotalEfectivo = totalEfectivo,
                TotalQR = totalQR,
                OtrosMetodos = otrosMetodos,
                IngresosTotales = ingresosTotales,
                TipoReservaAplicada = tipoReserva,
                ValorReservaConfigurado = valorReserva,
                MontoReservaDescontado = montoReserva,
                UtilidadNeta = utilidadNeta,
                TiempoProcesamientoMs = tiempoProcesamientoMs
            };

            return Ok(resumen);
        }
    }
}