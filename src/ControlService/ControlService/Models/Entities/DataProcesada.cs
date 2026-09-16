namespace ControlService.Models.Entities;

public class DataProcesada
{
    public int Id { get; set; }
    public int CargaArchivoId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = "Procesado";
    public string? MensajeError { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
