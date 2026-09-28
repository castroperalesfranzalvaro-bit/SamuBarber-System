namespace SamuBarber.Api.DTOs
{
    public class AgendarCitaDto
    {
        public int IdCliente { get; set; }
        public int IdBarbero { get; set; }
        public int IdServicio { get; set; }
        public DateTime FechaHora { get; set; }
    }

    public class RegistrarSinCitaDto
    {
        public string NombreCliente { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public int IdBarbero { get; set; }
        public int IdServicio { get; set; }
        public int DuracionMinutos { get; set; } = 40;
    }
}