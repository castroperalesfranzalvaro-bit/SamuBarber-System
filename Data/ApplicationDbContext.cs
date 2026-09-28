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
        public DbSet<Cliente> Clientes { get; set; }
        
        // 1. Agregar el DbSet de Usuarios
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<GastoCaja> GastosCaja { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito de la relación Venta -> DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta);

            modelBuilder.Entity<Cliente>().ToTable("cliente");
            
            // 2. Mapeo explícito de la tabla usuario en PostgreSQL
            modelBuilder.Entity<Usuario>().ToTable("usuario");
        }
    }
}