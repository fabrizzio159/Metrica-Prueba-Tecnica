using System.Data;
using CargaMasivaService.Data;
using CargaMasivaService.Interfaces;
using CargaMasivaService.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CargaMasivaService.Repositories;

public class CargaArchivoRepository : Repository<CargaArchivo>, ICargaArchivoRepository
{
    private readonly CargaMasivaDbContext _dbContext;

    public CargaArchivoRepository(CargaMasivaDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task ActualizarEstadoAsync(int id, string estado, string? mensajeError = null)
    {
        var paramError = new SqlParameter("@p2", SqlDbType.NVarChar)
        {
            Value = (object?)mensajeError ?? DBNull.Value
        };
        await _dbContext.Database.ExecuteSqlRawAsync(
            "EXEC sp_ActualizarEstadoCarga @p0, @p1, @p2",
            id, estado, paramError);
    }
}
