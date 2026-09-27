namespace SamuBarber.Api.DTOs
{
    public class ReporteMensualDto
    {
        public int Anio { get; set; }
        public int MesNumero { get; set; }
        public string MesNombre { get; set; } = string.Empty;
        public decimal IngresosBrutos { get; set; }
        public decimal EgresosComisionesBarberos { get; set; }
        public decimal EgresosReservaOperativa { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal UtilidadNeta { get; set; }
        public int TotalVentasRealizadas { get; set; }
    }
}