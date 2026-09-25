using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamuBarber.Api.Models
{
    [Table("cita")]
    public class Cita
    {
        [Key]
        [Column("id_cita")]
        public int Id { get; set; }

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
        public int DuracionTotalMin { get; set; } = 40;

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [Column("usuario_modificacion")]
        public string? UsuarioModificacion { get; set; }

        [Column("motivo_cancelacion")]
        public string? MotivoCancelacion { get; set; }
    }
}