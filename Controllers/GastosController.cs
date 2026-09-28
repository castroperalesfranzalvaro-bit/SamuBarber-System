using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GastosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GastosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Gastos/hoy
        [HttpGet("hoy")]
        public async Task<IActionResult> ObtenerGastosHoy()
        {
            var inicioDia = DateTime.UtcNow.Date;
            var finDia = inicioDia.AddDays(1);

            var gastos = await _context.GastosCaja
                .Where(g => g.FechaRegistro >= inicioDia && g.FechaRegistro < finDia)
                .OrderByDescending(g => g.FechaRegistro)
                .ToListAsync();

            var totalGastos = gastos.Sum(g => g.Monto);

            return Ok(new
            {
                totalGastos = totalGastos,
                lista = gastos
            });
        }

        // POST: api/Gastos/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarGasto([FromBody] RegistrarGastoDto dto)
        {
            if (dto.Monto <= 0)
                return BadRequest(new { mensaje = "El monto del gasto debe ser mayor a 0." });

            if (string.IsNullOrWhiteSpace(dto.Motivo))
                return BadRequest(new { mensaje = "Debe especificar un motivo para el gasto." });

            var gasto = new GastoCaja
            {
                Monto = dto.Monto,
                Motivo = dto.Motivo.Trim(),
                Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? "Varios" : dto.Categoria.Trim(),
                FechaRegistro = DateTime.UtcNow,
                IdUsuario = dto.IdUsuario
            };

            _context.GastosCaja.Add(gasto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Gasto registrado exitosamente.",
                gasto = gasto
            });
        }
    }

    public class RegistrarGastoDto
    {
        public decimal Monto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Categoria { get; set; } = "Varios";
        public int? IdUsuario { get; set; }
    }
}