using ControlService.Models.DTOs;

namespace ControlService.Interfaces;

public interface ICargaService
{
    Task<UploadResponse> UploadFileAsync(Stream fileStream, string fileName, string usuario);
    Task<IEnumerable<CargaHistorialDto>> GetHistorialAsync(string usuario);
    Task<CargaHistorialDto?> GetCargaByIdAsync(int id);
    Task<CargaDetalleDto?> GetDetalleAsync(int id);
    Task<bool> ReemplazarDetalleAsync(int detalleId);
}
