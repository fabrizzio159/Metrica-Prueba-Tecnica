namespace ControlService.Models.DTOs;

public class UploadResponse
{
    public int IdCarga { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
