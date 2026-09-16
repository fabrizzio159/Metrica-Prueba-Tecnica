namespace ControlService.Models.Messages;

public class CargaMensaje
{
    public int IdCarga { get; set; }
    public string RutaArchivo { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}
