using ControlService.Models.Entities;

namespace ControlService.Interfaces;

public interface ICargaArchivoRepository : IRepository<CargaArchivo>
{
    Task<IEnumerable<CargaArchivo>> GetByUsuarioAsync(string usuario);
}
