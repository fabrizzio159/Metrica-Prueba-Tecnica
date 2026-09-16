namespace CargaMasivaService.Models.Messages;

public class NotificacionMensaje
{
    public int IdCarga { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaFin { get; set; }
}
