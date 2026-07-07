using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "rabbitmq",
            UserName = _configuration["RabbitMq:User"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: "raffle.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(payload);
        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: "raffle.events",
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published event with routing key {RoutingKey}", routingKey);
    }
}
