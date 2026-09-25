namespace SamuBarber.Api.DTOs
{
    public class CancelarCitaDto
    {
        public int IdCita { get; set; }
        public string UsuarioResponsable { get; set; } = string.Empty; // Guardado para auditoría
        public string Motivo { get; set; } = string.Empty;
    }
}