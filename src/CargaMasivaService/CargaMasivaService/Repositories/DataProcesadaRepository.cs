using CargaMasivaService.Data;
using CargaMasivaService.Interfaces;
using CargaMasivaService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargaMasivaService.Repositories;

public class DataProcesadaRepository : Repository<DataProcesada>, IDataProcesadaRepository
{
    private readonly CargaMasivaDbContext _dbContext;

    public DataProcesadaRepository(CargaMasivaDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<bool> ExistePeriodoProductoAsync(string periodo, string codigoProducto)
    {
        return await _dbContext.DataProcesada
            .AnyAsync(d => d.Periodo == periodo && d.CodigoProducto == codigoProducto);
    }
}
