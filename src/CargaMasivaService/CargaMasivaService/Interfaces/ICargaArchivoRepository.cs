using CargaMasivaService.Models.Entities;

namespace CargaMasivaService.Interfaces;

public interface ICargaArchivoRepository : IRepository<CargaArchivo>
{
    Task ActualizarEstadoAsync(int id, string estado, string? mensajeError = null);
}
