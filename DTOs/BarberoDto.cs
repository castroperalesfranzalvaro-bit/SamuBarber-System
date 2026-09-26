namespace SamuBarber.Api.DTOs
{
    public class ResumenComisionBarberoDto
    {
        public int IdBarbero { get; set; }
        public int TotalCortesAtendidos { get; set; }
        public decimal TotalManoObraGenerada { get; set; }
        public decimal TotalComisionGanada { get; set; }
        public List<ServicioAtendidoDto> CortesRealizados { get; set; } = new();
    }

    public class ServicioAtendidoDto
    {
        public int IdVenta { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal SubtotalServicio { get; set; }
    }
}