using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("nombre_completo")]
        public string Nombre { get; set; } = string.Empty;

        [Column("telefono")]
        public string Telefono { get; set; } = "Sin teléfono"; // <-- Asigna este valor por defecto

        [Column("email")]
        public string? Email { get; set; }

        [Column("notas_preferencias")]
        public string? NotasPreferencia { get; set; }

        [Column("contador_visitas")]
        public int TotalAtenciones { get; set; } = 0;
    }
}