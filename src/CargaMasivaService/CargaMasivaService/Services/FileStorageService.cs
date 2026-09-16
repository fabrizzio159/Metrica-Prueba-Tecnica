using CargaMasivaService.Interfaces;

namespace CargaMasivaService.Services;

public class FileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FileStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Stream> DownloadFileAsync(string fileId)
    {
        var volumeUrl = _configuration["SeaweedFS:VolumeUrl"];
        var response = await _httpClient.GetAsync($"{volumeUrl}/{fileId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}
