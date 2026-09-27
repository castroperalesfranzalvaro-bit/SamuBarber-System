namespace SamuBarber.Api.DTOs
{
    public class ResumenCierreCajaDto
    {
        public string Fecha { get; set; } = string.Empty;
        public int TotalVentasCount { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalQR { get; set; }
        public decimal OtrosMetodos { get; set; }
        public decimal IngresosTotales { get; set; }
        public string TipoReservaAplicada { get; set; } = "porcentaje";
        public decimal ValorReservaConfigurado { get; set; }
        public decimal MontoReservaDescontado { get; set; }
        public decimal UtilidadNeta { get; set; }
        public double TiempoProcesamientoMs { get; set; }
    }
}