namespace SamuBarber.Api.DTOs
{
    public class ReprogramarCitaDto
    {
        public int IdCita { get; set; }
        public DateTime NuevaFechaHora { get; set; }
        public int? NuevoIdBarbero { get; set; } // Opcional, por si cambia de barbero
        public string UsuarioResponsable { get; set; } = string.Empty;
    }
}