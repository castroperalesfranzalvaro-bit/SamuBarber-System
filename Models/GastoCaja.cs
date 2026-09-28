using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("gasto_caja")]
    public class GastoCaja
    {
        [Key]
        [Column("id_gasto")]
        public int IdGasto { get; set; }

        [Required]
        [Column("monto")]
        public decimal Monto { get; set; }

        [Required]
        [Column("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [Column("categoria")]
        public string Categoria { get; set; } = "Varios";

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }
    }
}