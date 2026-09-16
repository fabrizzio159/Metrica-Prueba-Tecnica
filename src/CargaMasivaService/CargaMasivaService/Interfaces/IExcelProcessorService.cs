using CargaMasivaService.Models.Entities;

namespace CargaMasivaService.Interfaces;

public class ExcelRow
{
    public int RowNumber { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool IsEmpty { get; set; }
}

public interface IExcelProcessorService
{
    List<ExcelRow> ProcessExcel(Stream fileStream);
}
