using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos (Obtener todos los productos)
        [HttpGet]
        public async Task<IActionResult> ObtenerProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(productos);
        }

        // PUT: api/Productos/101/actualizar-stock (Ajustar stock desde inventario)
        [HttpPut("{id}/actualizar-stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int nuevoStock)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound("Producto no encontrado.");

            producto.Stock = nuevoStock;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Stock actualizado correctamente.", stockActual = producto.Stock });
        }
    }
}