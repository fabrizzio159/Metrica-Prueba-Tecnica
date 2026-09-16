namespace ControlService.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName);
}
