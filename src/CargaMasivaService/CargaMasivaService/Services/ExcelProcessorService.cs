using CargaMasivaService.Interfaces;
using ClosedXML.Excel;

namespace CargaMasivaService.Services;

public class ExcelProcessorService : IExcelProcessorService
{
    private readonly ILogger<ExcelProcessorService> _logger;

    public ExcelProcessorService(ILogger<ExcelProcessorService> logger)
    {
        _logger = logger;
    }

    public List<ExcelRow> ProcessExcel(Stream fileStream)
    {
        var rows = new List<ExcelRow>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.First();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int i = 2; i <= lastRow; i++)
        {
            var row = worksheet.Row(i);

            var periodo = row.Cell(1).GetString().Trim();
            var codigoProducto = row.Cell(2).GetString().Trim();
            var nombreProducto = row.Cell(3).GetString().Trim();
            var precioStr = row.Cell(4).GetString().Trim();

            var isEmpty = string.IsNullOrEmpty(periodo)
                       && string.IsNullOrEmpty(codigoProducto)
                       && string.IsNullOrEmpty(nombreProducto)
                       && string.IsNullOrEmpty(precioStr);

            if (isEmpty)
            {
                _logger.LogDebug("Fila {Row} vacía, omitida", i);
                continue;
            }

            if (string.IsNullOrEmpty(periodo)) periodo = "SIN_PERIODO";
            if (string.IsNullOrEmpty(codigoProducto)) codigoProducto = "SIN_CODIGO";
            if (string.IsNullOrEmpty(nombreProducto)) nombreProducto = "SIN_NOMBRE";

            decimal precio = 0;
            if (!string.IsNullOrEmpty(precioStr))
                decimal.TryParse(precioStr, out precio);

            rows.Add(new ExcelRow
            {
                RowNumber = i,
                Periodo = periodo,
                CodigoProducto = codigoProducto,
                NombreProducto = nombreProducto,
                Precio = precio
            });
        }

        _logger.LogInformation("Excel procesado: {Count} filas válidas encontradas", rows.Count);
        return rows;
    }
}
