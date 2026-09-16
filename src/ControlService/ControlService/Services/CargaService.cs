using ControlService.Data;
using ControlService.Interfaces;
using ControlService.Models.DTOs;
using ControlService.Models.Entities;
using ControlService.Models.Messages;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Services;

public class CargaService : ICargaService
{
    private readonly ICargaArchivoRepository _cargaRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CargaService> _logger;
    private readonly ControlDbContext _dbContext;

    public CargaService(
        ICargaArchivoRepository cargaRepository,
        IFileStorageService fileStorage,
        IMessagePublisher messagePublisher,
        IConfiguration configuration,
        ILogger<CargaService> logger,
        ControlDbContext dbContext)
    {
        _cargaRepository = cargaRepository;
        _fileStorage = fileStorage;
        _messagePublisher = messagePublisher;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UploadResponse> UploadFileAsync(Stream fileStream, string fileName, string usuario)
    {
        var maxSizeMb = _configuration.GetValue<int>("FileUpload:MaxSizeMB", 10);
        if (fileStream.Length > maxSizeMb * 1024 * 1024)
            throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {maxSizeMb}MB.");

        var extension = Path.GetExtension(fileName)?.ToLower();
        if (extension != ".xlsx")
            throw new ArgumentException("Solo se permiten archivos con extensión .xlsx");

        var fileId = await _fileStorage.UploadFileAsync(fileStream, fileName);

        var carga = new CargaArchivo
        {
            NombreArchivo = fileName,
            Usuario = usuario,
            Estado = "Pendiente",
            RutaArchivo = fileId,
            FechaRegistro = DateTime.UtcNow
        };

        var idCarga = await _cargaRepository.AddAsync(carga);

        var mensaje = new CargaMensaje
        {
            IdCarga = idCarga,
            RutaArchivo = fileId,
            Usuario = usuario
        };

        await _messagePublisher.PublishAsync(mensaje, "carga_masiva");

        _logger.LogInformation("Carga {IdCarga} registrada y publicada para usuario {Usuario}", idCarga, usuario);

        return new UploadResponse
        {
            IdCarga = idCarga,
            NombreArchivo = fileName,
            Estado = "Pendiente",
            Mensaje = "Archivo recibido. El procesamiento ha sido encolado."
        };
    }

    public async Task<IEnumerable<CargaHistorialDto>> GetHistorialAsync(string usuario)
    {
        var cargas = await _cargaRepository.GetByUsuarioAsync(usuario);
        return cargas.Select(c => new CargaHistorialDto
        {
            Id = c.Id,
            NombreArchivo = c.NombreArchivo,
            Estado = c.Estado,
            FechaRegistro = c.FechaRegistro,
            FechaFin = c.FechaFin,
            Periodo = c.Periodo,
            MensajeError = c.MensajeError,
            TotalFilas = c.TotalFilas
        });
    }

    public async Task<CargaHistorialDto?> GetCargaByIdAsync(int id)
    {
        var carga = await _cargaRepository.GetByIdAsync(id);
        if (carga == null) return null;

        return new CargaHistorialDto
        {
            Id = carga.Id,
            NombreArchivo = carga.NombreArchivo,
            Estado = carga.Estado,
            FechaRegistro = carga.FechaRegistro,
            FechaFin = carga.FechaFin,
            Periodo = carga.Periodo,
            MensajeError = carga.MensajeError,
            TotalFilas = carga.TotalFilas
        };
    }

    public async Task<CargaDetalleDto?> GetDetalleAsync(int id)
    {
        var carga = await _cargaRepository.GetByIdAsync(id);
        if (carga == null) return null;

        var datos = await _dbContext.DataProcesada
            .Where(d => d.CargaArchivoId == id)
            .OrderBy(d => d.Id)
            .Select(d => new DataProcesadaDto
            {
                Id = d.Id,
                Periodo = d.Periodo,
                CodigoProducto = d.CodigoProducto,
                NombreProducto = d.NombreProducto,
                Precio = d.Precio,
                Estado = d.Estado,
                MensajeError = d.MensajeError,
                FechaRegistro = d.FechaRegistro
            })
            .ToListAsync();

        return new CargaDetalleDto
        {
            Id = carga.Id,
            NombreArchivo = carga.NombreArchivo,
            Estado = carga.Estado,
            FechaRegistro = carga.FechaRegistro,
            FechaFin = carga.FechaFin,
            Periodo = carga.Periodo,
            MensajeError = carga.MensajeError,
            TotalFilas = carga.TotalFilas,
            Datos = datos
        };
    }

    public async Task<bool> ReemplazarDetalleAsync(int detalleId)
    {
        var errorRow = await _dbContext.DataProcesada.FindAsync(detalleId);
        if (errorRow == null || errorRow.Estado != "Error") return false;

        var existente = await _dbContext.DataProcesada
            .FirstOrDefaultAsync(d => d.Periodo == errorRow.Periodo
                && d.CodigoProducto == errorRow.CodigoProducto
                && d.Estado == "Procesado");

        if (existente != null)
            _dbContext.DataProcesada.Remove(existente);

        errorRow.Estado = "Procesado";
        errorRow.MensajeError = null;
        await _dbContext.SaveChangesAsync();

        var tieneErrores = await _dbContext.DataProcesada
            .AnyAsync(d => d.CargaArchivoId == errorRow.CargaArchivoId && d.Estado == "Error");

        if (!tieneErrores)
        {
            var carga = await _cargaRepository.GetByIdAsync(errorRow.CargaArchivoId);
            if (carga != null && carga.Estado == "ConErrores")
            {
                carga.Estado = "Finalizado";
                carga.MensajeError = null;
                await _cargaRepository.UpdateAsync(carga);
            }
        }

        return true;
    }
}
