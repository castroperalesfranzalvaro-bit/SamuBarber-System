namespace SamuBarber.Api.DTOs
{
    public class ActualizarNotasDto
    {
        public string NotasPreferencia { get; set; } = string.Empty;
    }

    public class HistorialClienteDto
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string NotasPreferencia { get; set; } = string.Empty;
        public int TotalAtenciones { get; set; }
        public bool ElegibleDescuento { get; set; }
        public List<VentaHistorialDto> HistorialVentas { get; set; } = new();
    }

    public class VentaHistorialDto
    {
        public int IdVenta { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
    }
}