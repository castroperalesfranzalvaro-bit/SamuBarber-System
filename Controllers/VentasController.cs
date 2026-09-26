using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data; // Ajusta según tu DbContext
using SamuBarber.Api.DTOs;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TAREA 2: Lógica de Cobro con Efectivo y QR
        [HttpPost("registrar-cobro")]
        public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarVentaDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Debe agregar al menos un ítem al detalle de la venta.");

            decimal totalCalculado = dto.Items.Sum(i => i.Cantidad * i.PrecioUnitario);
            decimal cambio = 0;

            if (dto.MetodoPago == "Efectivo")
            {
                if (dto.MontoRecibido < totalCalculado)
                    return BadRequest("El monto recibido en efectivo es menor al total de la venta.");
                
                cambio = dto.MontoRecibido - totalCalculado;
            }
            else // QR
            {
                dto.MontoRecibido = totalCalculado;
                cambio = 0;
            }

            var nuevaVenta = new Venta
            {
                IdBarbero = dto.IdBarbero,
                IdCliente = dto.IdCliente,
                FechaVenta = DateTime.UtcNow,
                MetodoPago = dto.MetodoPago,
                Total = totalCalculado,
                MontoRecibido = dto.MontoRecibido,
                Cambio = cambio,
                Detalles = dto.Items.Select(i => new DetalleVenta
                {
                    TipoItem = i.TipoItem,
                    IdItem = i.IdItem,
                    NombreItem = i.NombreItem,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Cantidad * i.PrecioUnitario
                }).ToList()
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Venta registrada exitosamente",
                idVenta = nuevaVenta.IdVenta,
                total = nuevaVenta.Total,
                cambio = nuevaVenta.Cambio
            });
        }

        // TAREA 3: Crear Módulo Generador de Comprobante Digital
        [HttpGet("comprobante/{idVenta}")]
        public async Task<IActionResult> ObtenerComprobante(int idVenta)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null)
                return NotFound("No se encontró la venta especificada.");

            var comprobante = new
            {
                Establecimiento = "Samu Barber Shop",
                NumeroComprobante = $"REC-{venta.IdVenta:D6}",
                Fecha = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                BarberoId = venta.IdBarbero,
                ClienteId = venta.IdCliente.HasValue ? venta.IdCliente.Value.ToString() : "Cliente General",
                MetodoPago = venta.MetodoPago,
                Items = venta.Detalles.Select(d => new
                {
                    Descripcion = $"[{d.TipoItem}] {d.NombreItem}",
                    Cant = d.Cantidad,
                    Precio = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }),
                Total = venta.Total,
                MontoRecibido = venta.MontoRecibido,
                Cambio = venta.Cambio
            };

            return Ok(comprobante);
        }
    }
}