namespace ControlService.Models.DTOs;

public class CargaDetalleDto
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Periodo { get; set; }
    public string? MensajeError { get; set; }
    public int TotalFilas { get; set; }
    public List<DataProcesadaDto> Datos { get; set; } = new();
}

public class DataProcesadaDto
{
    public int Id { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
    public DateTime FechaRegistro { get; set; }
}
