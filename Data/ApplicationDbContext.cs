using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Models;

namespace SamuBarber.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
    }
}