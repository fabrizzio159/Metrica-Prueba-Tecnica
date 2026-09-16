using NotificacionService.Interfaces;
using NotificacionService.Models.Messages;

namespace NotificacionService.Services;

public class NotificacionProcessingService : INotificacionService
{
    private readonly ICargaArchivoRepository _cargaRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificacionProcessingService> _logger;

    public NotificacionProcessingService(
        ICargaArchivoRepository cargaRepository,
        IEmailService emailService,
        ILogger<NotificacionProcessingService> logger)
    {
        _cargaRepository = cargaRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcesarNotificacionAsync(NotificacionMensaje mensaje)
    {
        _logger.LogInformation("Procesando notificación para carga {IdCarga}", mensaje.IdCarga);

        var carga = await _cargaRepository.GetByIdAsync(mensaje.IdCarga);

        var subject = $"Carga Masiva #{mensaje.IdCarga} - Proceso Finalizado";
        var body = $@"
            <h2>Notificación de Carga Masiva</h2>
            <p>Estimado/a usuario,</p>
            <p>El procesamiento de su carga masiva ha finalizado.</p>
            <table border='1' cellpadding='8' cellspacing='0'>
                <tr><td><strong>ID Carga</strong></td><td>{mensaje.IdCarga}</td></tr>
                <tr><td><strong>Archivo</strong></td><td>{carga?.NombreArchivo ?? "N/A"}</td></tr>
                <tr><td><strong>Estado</strong></td><td>{carga?.Estado ?? "Finalizado"}</td></tr>
                <tr><td><strong>Fecha Fin</strong></td><td>{mensaje.FechaFin:yyyy-MM-dd HH:mm:ss}</td></tr>
                {(carga?.MensajeError != null ? $"<tr><td><strong>Observaciones</strong></td><td>{carga.MensajeError}</td></tr>" : "")}
            </table>
            <p>Saludos,<br/>Sistema de Carga Masiva</p>";

        try
        {
            await _emailService.SendEmailAsync(mensaje.Usuario, subject, body);
            if (carga?.Estado != "ConErrores")
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Notificado");
            _logger.LogInformation("Notificación enviada para carga {IdCarga}", mensaje.IdCarga);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación para carga {IdCarga}", mensaje.IdCarga);
            if (carga?.Estado != "ConErrores")
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Notificado");
        }
    }
}
