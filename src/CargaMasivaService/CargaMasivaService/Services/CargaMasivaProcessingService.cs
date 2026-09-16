using CargaMasivaService.Interfaces;
using CargaMasivaService.Models.Entities;
using CargaMasivaService.Models.Messages;

namespace CargaMasivaService.Services;

public class CargaMasivaProcessingService : ICargaMasivaService
{
    private readonly ICargaArchivoRepository _cargaRepository;
    private readonly IDataProcesadaRepository _dataProcesadaRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IExcelProcessorService _excelProcessor;
    private readonly IFileStorageService _fileStorage;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CargaMasivaProcessingService> _logger;

    public CargaMasivaProcessingService(
        ICargaArchivoRepository cargaRepository,
        IDataProcesadaRepository dataProcesadaRepository,
        IAuditoriaRepository auditoriaRepository,
        IExcelProcessorService excelProcessor,
        IFileStorageService fileStorage,
        IMessagePublisher messagePublisher,
        ILogger<CargaMasivaProcessingService> logger)
    {
        _cargaRepository = cargaRepository;
        _dataProcesadaRepository = dataProcesadaRepository;
        _auditoriaRepository = auditoriaRepository;
        _excelProcessor = excelProcessor;
        _fileStorage = fileStorage;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task ProcesarCargaAsync(CargaMensaje mensaje)
    {
        _logger.LogInformation("Iniciando procesamiento de carga {IdCarga}", mensaje.IdCarga);

        await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "EnProceso");

        try
        {
            using var fileStream = await _fileStorage.DownloadFileAsync(mensaje.RutaArchivo);
            var rows = _excelProcessor.ProcessExcel(fileStream);

            if (rows.Count == 0)
            {
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Finalizado", "El archivo no contiene datos válidos.");
                await PublicarNotificacionAsync(mensaje);
                return;
            }

            var periodos = rows.Select(r => r.Periodo).Distinct().ToList();

            var carga = await _cargaRepository.GetByIdAsync(mensaje.IdCarga);
            if (carga != null)
            {
                if (periodos.Count > 0) carga.Periodo = string.Join(", ", periodos);
                carga.TotalFilas = rows.Count;
                await _cargaRepository.UpdateAsync(carga);
            }

            int insertados = 0;
            int rechazados = 0;

            foreach (var row in rows)
            {
                var existe = await _dataProcesadaRepository.ExistePeriodoProductoAsync(row.Periodo, row.CodigoProducto);

                if (existe)
                {
                    await _dataProcesadaRepository.AddAsync(new DataProcesada
                    {
                        CargaArchivoId = mensaje.IdCarga,
                        Periodo = row.Periodo,
                        CodigoProducto = row.CodigoProducto,
                        NombreProducto = row.NombreProducto,
                        Precio = row.Precio,
                        Estado = "Error",
                        MensajeError = $"Ya existe '{row.CodigoProducto}' para el periodo '{row.Periodo}'.",
                        FechaRegistro = DateTime.UtcNow
                    });

                    await _auditoriaRepository.AddAsync(new AuditoriaFallo
                    {
                        CargaArchivoId = mensaje.IdCarga,
                        Fila = row.RowNumber,
                        CodigoProducto = row.CodigoProducto,
                        MotivoRechazo = "DuplicadoPeriodoProducto",
                        DetalleFallo = $"Ya existe '{row.CodigoProducto}' para el periodo '{row.Periodo}'.",
                        FechaRegistro = DateTime.UtcNow
                    });
                    rechazados++;
                    continue;
                }

                await _dataProcesadaRepository.AddAsync(new DataProcesada
                {
                    CargaArchivoId = mensaje.IdCarga,
                    Periodo = row.Periodo,
                    CodigoProducto = row.CodigoProducto,
                    NombreProducto = row.NombreProducto,
                    Precio = row.Precio,
                    Estado = "Procesado",
                    FechaRegistro = DateTime.UtcNow
                });
                insertados++;
            }

            _logger.LogInformation("Carga {IdCarga}: {Insertados} insertados, {Rechazados} rechazados",
                mensaje.IdCarga, insertados, rechazados);

            if (rechazados > 0)
            {
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "ConErrores",
                    $"{rechazados} de {rows.Count} filas duplicadas");
            }
            else
            {
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Cargado");
                await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Finalizado");
            }

            await PublicarNotificacionAsync(mensaje);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando carga {IdCarga}", mensaje.IdCarga);
            await _cargaRepository.ActualizarEstadoAsync(mensaje.IdCarga, "Finalizado", ex.Message);
            await PublicarNotificacionAsync(mensaje);
        }
    }

    private async Task PublicarNotificacionAsync(CargaMensaje mensaje)
    {
        var notificacion = new NotificacionMensaje
        {
            IdCarga = mensaje.IdCarga,
            Usuario = mensaje.Usuario,
            FechaFin = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync(notificacion, "notificaciones");
    }
}
