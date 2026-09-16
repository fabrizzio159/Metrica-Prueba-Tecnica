using System.Text;
using System.Text.Json;
using NotificacionService.Interfaces;
using NotificacionService.Models.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificacionService.Workers;

public class NotificacionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificacionWorker> _logger;

    public NotificacionWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<NotificacionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificacionWorker iniciado. Esperando mensajes...");

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        IConnection? connection = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                break;
            }
            catch
            {
                _logger.LogWarning("RabbitMQ no disponible. Reintentando en 5 segundos...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection == null) return;

        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "notificaciones",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var mensaje = JsonSerializer.Deserialize<NotificacionMensaje>(body);

                if (mensaje != null)
                {
                    _logger.LogInformation("Notificación recibida para carga {IdCarga}", mensaje.IdCarga);

                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<INotificacionService>();
                    await service.ProcesarNotificacionAsync(mensaje);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje de notificación");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "notificaciones",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
