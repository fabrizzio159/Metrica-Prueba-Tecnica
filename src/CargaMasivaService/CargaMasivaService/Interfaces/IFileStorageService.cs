namespace CargaMasivaService.Interfaces;

public interface IFileStorageService
{
    Task<Stream> DownloadFileAsync(string fileId);
}
