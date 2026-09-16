using ControlService.Data;
using ControlService.Interfaces;
using ControlService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Repositories;

public class CargaArchivoRepository : Repository<CargaArchivo>, ICargaArchivoRepository
{
    private readonly ControlDbContext _dbContext;

    public CargaArchivoRepository(ControlDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<CargaArchivo>> GetByUsuarioAsync(string usuario)
    {
        return await _dbContext.CargaArchivos
            .Where(c => c.Usuario == usuario)
            .OrderByDescending(c => c.FechaRegistro)
            .ToListAsync();
    }
}
