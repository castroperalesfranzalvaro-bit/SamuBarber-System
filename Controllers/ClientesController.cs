using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener la lista completa de clientes
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(clientes);
        }

        // TAREA 1 (Backend): Obtener Perfil, Notas y Historial de Cortes/Ventas del Cliente
        [HttpGet("{idCliente}/historial")]
        public async Task<IActionResult> ObtenerHistorial(int idCliente)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var ventas = await _context.Ventas
                .Where(v => v.IdCliente == idCliente)
                .Include(v => v.Detalles)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            // Regla TAREA 3: Si la siguiente visita es múltiplo de 5 (5ta, 10ma, 15ta...), aplica 30% descuento
            bool elegibleDescuento = (cliente.TotalAtenciones + 1) % 5 == 0;

            var dto = new HistorialClienteDto
            {
                IdCliente = cliente.IdCliente,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                NotasPreferencia = cliente.NotasPreferencia ?? "Sin notas registradas.",
                TotalAtenciones = cliente.TotalAtenciones,
                ElegibleDescuento = elegibleDescuento,
                HistorialVentas = ventas.Select(v => new VentaHistorialDto
                {
                    IdVenta = v.IdVenta,
                    Fecha = v.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                    Total = v.Total,
                    MetodoPago = v.MetodoPago,
                    Items = v.Detalles.Select(d => $"{d.Cantidad}x {d.NombreItem} (Bs. {d.Subtotal})").ToList()
                }).ToList()
            };

            return Ok(dto);
        }

        // TAREA 1 (Backend): Actualizar las notas sobre el estilo de corte habitual (RF-04)
        [HttpPut("{idCliente}/notas")]
        public async Task<IActionResult> ActualizarNotas(int idCliente, [FromBody] ActualizarNotasDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            cliente.NotasPreferencia = dto.NotasPreferencia;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Notas de preferencia actualizadas correctamente." });
        }
    }
}