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
       // Reemplaza o actualiza tu método RegistrarCobro en VentasController.cs
[HttpPost("registrar-cobro")]
public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarVentaDto dto)
{
    if (dto.Items == null || !dto.Items.Any())
        return BadRequest("Debe agregar al menos un ítem al detalle de la venta.");

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Validar y Descontar Stock de los Productos
        foreach (var item in dto.Items)
        {
            if (string.Equals(item.TipoItem, "Producto", StringComparison.OrdinalIgnoreCase))
            {
                var producto = await _context.Productos.FindAsync(item.IdItem);

                if (producto == null)
                    return BadRequest($"El producto '{item.NombreItem}' (ID: {item.IdItem}) no existe.");

                if (producto.Stock < item.Cantidad)
                    return BadRequest($"Stock insuficiente para '{producto.NombreProducto}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}.");

                producto.Stock -= item.Cantidad; // Descontar del inventario
            }
        }

        decimal totalCalculado = dto.Items.Sum(i => i.Cantidad * i.PrecioUnitario);
        bool descuentoAplicado = false;

        // Regla de fidelización de clientes
        if (dto.IdCliente.HasValue && dto.IdCliente.Value > 0)
        {
            var cliente = await _context.Clientes.FindAsync(dto.IdCliente.Value);
            if (cliente != null)
            {
                cliente.TotalAtenciones += 1;
                if (cliente.TotalAtenciones % 5 == 0)
                {
                    totalCalculado *= 0.70m;
                    descuentoAplicado = true;
                }
            }
        }

        decimal cambio = 0;
        if (dto.MetodoPago == "Efectivo")
        {
            if (dto.MontoRecibido < totalCalculado)
                return BadRequest($"El monto recibido es menor al total (Total: Bs. {totalCalculado:F2}).");

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
        await transaction.CommitAsync();

        return Ok(new
        {
            mensaje = descuentoAplicado ? "¡Venta registrada con 30% de descuento por 5ta visita!" : "Venta registrada exitosamente",
            idVenta = nuevaVenta.IdVenta,
            total = nuevaVenta.Total,
            cambio = nuevaVenta.Cambio,
            descuentoAplicado = descuentoAplicado
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return StatusCode(500, $"Error al procesar la venta: {ex.Message}");
    }
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