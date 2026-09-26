namespace SamuBarber.Api.DTOs
{
    public class ItemVentaDto
    {
        public string TipoItem { get; set; } // "Servicio" o "Producto"
        public int IdItem { get; set; }
        public string NombreItem { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class RegistrarVentaDto
    {
        public int IdBarbero { get; set; }
        public int? IdCliente { get; set; }
        public string MetodoPago { get; set; } // "Efectivo" o "QR"
        public decimal MontoRecibido { get; set; }
        public List<ItemVentaDto> Items { get; set; } = new();
    }
}