namespace CargaMasivaService.Models.Entities;

public class AuditoriaFallo
{
    public int Id { get; set; }
    public int CargaArchivoId { get; set; }
    public int? Fila { get; set; }
    public string? CodigoProducto { get; set; }
    public string MotivoRechazo { get; set; } = string.Empty;
    public string? DetalleFallo { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public CargaArchivo CargaArchivo { get; set; } = null!;
}
