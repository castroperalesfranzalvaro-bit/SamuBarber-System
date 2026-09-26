using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.DTOs;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarberosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BarberosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TAREA 1 & 3: Obtener comisiones del día para un barbero específico (RBAC - Solo ve sus datos)
        [HttpGet("{idBarbero}/comisiones-dia")]
        public async Task<IActionResult> ObtenerComisionesDia(int idBarbero)
        {
            // Definir el rango del día actual en UTC / Fecha Local
            var hoyInicio = DateTime.UtcNow.Date;
            var hoyFin = hoyInicio.AddDays(1);

            // Consultar las ventas asociadas únicamente al barbero en sesión
            var ventasBarbero = await _context.Ventas
                .Where(v => v.IdBarbero == idBarbero && v.FechaVenta >= hoyInicio && v.FechaVenta < hoyFin)
                .Include(v => v.Detalles)
                .ToListAsync();

            var listaServicios = new List<ServicioAtendidoDto>();
            decimal totalManoObra = 0;

            foreach (var venta in ventasBarbero)
            {
                // Filtrar solo los ítems correspondientes a Servicios (Mano de Obra)
                var servicios = venta.Detalles.Where(d => d.TipoItem == "Servicio").ToList();

                foreach (var s in servicios)
                {
                    totalManoObra += s.Subtotal;
                    listaServicios.Add(new ServicioAtendidoDto
                    {
                        IdVenta = venta.IdVenta,
                        Fecha = venta.FechaVenta.ToString("HH:mm"),
                        NombreServicio = s.NombreItem,
                        Cantidad = s.Cantidad,
                        SubtotalServicio = s.Subtotal
                    });
                }
            }

            // TAREA 1: Criterio de Aceptación 2 - Calculo exacto del 50% sobre mano de obra
            decimal comisionGanada = totalManoObra * 0.50m;

            // Retorna ÚNICAMENTE la información del barbero (Seguridad RBAC - Oculta montos globales del local)
            var resumenDto = new ResumenComisionBarberoDto
            {
                IdBarbero = idBarbero,
                TotalCortesAtendidos = listaServicios.Sum(s => s.Cantidad),
                TotalManoObraGenerada = totalManoObra,
                TotalComisionGanada = comisionGanada,
                CortesRealizados = listaServicios
            };

            return Ok(resumenDto);
        }
    }
}