using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("detalle_venta")]
    public class DetalleVenta
    {
        [Key]
        [Column("id_detalle")]
        public int IdDetalle { get; set; }

        [Column("id_venta")]
        public int IdVenta { get; set; }

        // --- AGREGAR ESTA PROPIEDAD Y SU ATRIBUTO ---
        [ForeignKey("IdVenta")]
        public Venta? Venta { get; set; }
        // ---------------------------------------------

        [Column("tipo_item")]
        public string TipoItem { get; set; }

        [Column("id_item")]
        public int IdItem { get; set; }

        [Column("nombre_item")]
        public string NombreItem { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }
    }
}