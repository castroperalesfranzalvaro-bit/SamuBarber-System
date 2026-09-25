using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Forzar a Entity Framework a usar la tabla 'citas' en minúsculas en PostgreSQL
            modelBuilder.Entity<Cita>().ToTable("cita");
        }
    }
}