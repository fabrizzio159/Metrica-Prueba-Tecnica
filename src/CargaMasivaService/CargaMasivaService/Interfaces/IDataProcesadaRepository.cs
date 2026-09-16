using CargaMasivaService.Models.Entities;

namespace CargaMasivaService.Interfaces;

public interface IDataProcesadaRepository : IRepository<DataProcesada>
{
    Task<bool> ExistePeriodoProductoAsync(string periodo, string codigoProducto);
}
