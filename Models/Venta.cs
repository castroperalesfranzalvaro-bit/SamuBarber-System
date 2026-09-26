using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("venta")]
    public class Venta
    {
        [Key]
        [Column("id_venta")]
        public int IdVenta { get; set; }

        [Column("id_barbero")]
        public int IdBarbero { get; set; }

        [Column("id_cliente")]
        public int? IdCliente { get; set; }

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Column("metodo_pago")]
        public string MetodoPago { get; set; } // "Efectivo" o "QR"

        [Column("total")]
        public decimal Total { get; set; }

        [Column("monto_recibido")]
        public decimal MontoRecibido { get; set; }

        [Column("cambio")]
        public decimal Cambio { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}