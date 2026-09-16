namespace CargaMasivaService.Models.Entities;

public class CargaArchivo
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Pendiente";
    public string? Periodo { get; set; }
    public string? RutaArchivo { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? MensajeError { get; set; }
    public int TotalFilas { get; set; }
}
