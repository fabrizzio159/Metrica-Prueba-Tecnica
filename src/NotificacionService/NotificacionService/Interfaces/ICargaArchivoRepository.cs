using NotificacionService.Models.Entities;

namespace NotificacionService.Interfaces;

public interface ICargaArchivoRepository : IRepository<CargaArchivo>
{
    Task ActualizarEstadoAsync(int id, string estado);
}
