using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("rol")]
        public string Rol { get; set; } = "Barbero";

        [Column("email")]
        public string? Email { get; set; }

        [Column("password")]
        public string? Password { get; set; }
    }
}