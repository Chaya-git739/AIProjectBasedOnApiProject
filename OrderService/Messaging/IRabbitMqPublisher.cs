namespace OrderService.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default);
}
