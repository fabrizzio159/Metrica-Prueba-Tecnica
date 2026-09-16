using Microsoft.EntityFrameworkCore;
using NotificacionService.Data;
using NotificacionService.Interfaces;
using NotificacionService.Models.Entities;

namespace NotificacionService.Repositories;

public class CargaArchivoRepository : Repository<CargaArchivo>, ICargaArchivoRepository
{
    private readonly NotificacionDbContext _dbContext;

    public CargaArchivoRepository(NotificacionDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task ActualizarEstadoAsync(int id, string estado)
    {
        var carga = await _dbContext.CargaArchivos.FindAsync(id);
        if (carga != null)
        {
            carga.Estado = estado;
            carga.FechaFin = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
