using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("nombre_producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("precio_compra")]
        public decimal PrecioCompra { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [Column("stock_minimo")]
        public int StockMinimo { get; set; }
    }
}