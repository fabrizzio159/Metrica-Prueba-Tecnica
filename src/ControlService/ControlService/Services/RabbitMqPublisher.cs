using System.Text;
using System.Text.Json;
using ControlService.Interfaces;
using RabbitMQ.Client;

namespace ControlService.Services;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    private RabbitMqPublisher(IConnection connection, IChannel channel, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMqPublisher> CreateAsync(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        return new RabbitMqPublisher(connection, channel, logger);
    }

    public async Task PublishAsync<T>(T message, string queueName)
    {
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { Persistent = true };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Mensaje publicado en cola {Queue}: {Message}", queueName, json);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
