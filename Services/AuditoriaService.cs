namespace SamuBarber.Api.Services
{
    public class AuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;
        }

        public void RegistrarAnulacionOModificacion(int idCita, string accion, string usuario, string detalle)
        {
            string mensajeLog = $"[AUDITORÍA - {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cita ID: {idCita} | Acción: {accion} | Usuario Responsable: {usuario} | Detalle: {detalle}";
            
            // Log en consola / servidor
            _logger.LogInformation(mensajeLog);

            // Guardar en un archivo físico de auditoría logs/auditoria_citas.txt
            try
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, "auditoria_citas.log");
                File.AppendAllText(filePath, mensajeLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error guardando log en archivo: {ex.Message}");
            }
        }
    }
}