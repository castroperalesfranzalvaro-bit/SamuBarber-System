using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("cita")]
    public class Cita
    {
        [Key]
        [Column("id_cita")]
        public int IdCita { get; set; }

        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("id_barbero")]
        public int IdBarbero { get; set; }

        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Agendada";

        [Column("duracion_total_min")]
        public int DuracionTotalMin { get; set; }
    }

    [Table("servicio")]
    public class Servicio
    {
        [Key]
        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [Column("nombre_servicio")]
        public string NombreServicio { get; set; } = string.Empty;

        [Column("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }
    }
}