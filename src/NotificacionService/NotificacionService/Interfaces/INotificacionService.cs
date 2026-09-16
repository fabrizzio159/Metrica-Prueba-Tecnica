using NotificacionService.Models.Messages;

namespace NotificacionService.Interfaces;

public interface INotificacionService
{
    Task ProcesarNotificacionAsync(NotificacionMensaje mensaje);
}
