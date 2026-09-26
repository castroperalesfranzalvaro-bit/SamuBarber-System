using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }

        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Mapeo explícito de la relación Venta -> DetalleVenta
    modelBuilder.Entity<DetalleVenta>()
        .HasOne(d => d.Venta)
        .WithMany(v => v.Detalles)
        .HasForeignKey(d => d.IdVenta);
}
        
    }
}