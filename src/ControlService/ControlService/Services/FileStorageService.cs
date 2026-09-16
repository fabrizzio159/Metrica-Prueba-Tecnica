using System.Text.Json;
using ControlService.Interfaces;

namespace ControlService.Services;

public class FileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(HttpClient httpClient, IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var masterUrl = _configuration["SeaweedFS:MasterUrl"];
        var assignResponse = await _httpClient.GetAsync($"{masterUrl}/dir/assign");
        assignResponse.EnsureSuccessStatusCode();

        var assignContent = await assignResponse.Content.ReadAsStringAsync();
        var assignResult = JsonSerializer.Deserialize<JsonElement>(assignContent);

        var fid = assignResult.GetProperty("fid").GetString()!;
        var volumeUrl = assignResult.GetProperty("url").GetString()!;

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var uploadResponse = await _httpClient.PostAsync($"http://{volumeUrl}/{fid}", content);
        uploadResponse.EnsureSuccessStatusCode();

        _logger.LogInformation("Archivo {FileName} subido a SeaweedFS con fid {Fid}", fileName, fid);
        return fid;
    }

    public async Task<Stream> DownloadFileAsync(string fileId)
    {
        var volumeUrl = _configuration["SeaweedFS:VolumeUrl"];
        var response = await _httpClient.GetAsync($"{volumeUrl}/{fileId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
}
