namespace ControlService.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
    Task<Stream> DownloadFileAsync(string fileId);
}
