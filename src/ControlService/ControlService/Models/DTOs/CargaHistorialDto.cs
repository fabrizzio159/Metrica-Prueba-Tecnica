namespace ControlService.Models.DTOs;

public class CargaHistorialDto
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Periodo { get; set; }
    public string? MensajeError { get; set; }
    public int TotalFilas { get; set; }
}
