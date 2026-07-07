using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Services;

namespace NotificationService.Messaging;

public sealed class OrderStatusChangedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public OrderStatusChangedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderStatusChangedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "rabbitmq",
            UserName = _configuration["RabbitMq:User"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync("raffle.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        var args = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "raffle.events",
            ["x-dead-letter-routing-key"] = "order.status-changed.dead"
        };

        await _channel.QueueDeclareAsync(
            queue: "notifications.order-status",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync("notifications.order-status", "raffle.events", "order.status-changed", cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync("notifications.order-status.dlq", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("notifications.order-status.dlq", "raffle.events", "order.status-changed.dead", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<OrderStatusChangedEvent>(body);

            if (evt == null)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IProcessedMessageStore>();
            if (!store.TryMarkProcessed(evt.MessageId))
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                return;
            }

            var retryCount = GetRetryCount(ea.BasicProperties);

            try
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var to = $"user-{evt.UserId}@placeholder.local";
                var subject = evt.Status == "Confirmed" ? "הזמנה אושרה" : "הזמנה נדחתה";
                var message = evt.Status == "Confirmed"
                    ? $"הזמנה מספר {evt.OrderId} אושרה בהצלחה."
                    : $"הזמנה מספר {evt.OrderId} נדחתה. סיבה: {evt.Reason}";

                _logger.LogWarning("[Saga] Handling order.status-changed for OrderId={OrderId} Status={Status} CorrelationId={CorrelationId}", evt.OrderId, evt.Status, evt.CorrelationId);

                await notificationService.SendAsync(to, subject, message);
                _logger.LogWarning("[Saga] Notification sent for OrderId={OrderId} Status={Status}", evt.OrderId, evt.Status);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification consumer failed for order {OrderId} retry={Retry}", evt.OrderId, retryCount);

                if (retryCount < 3)
                {
                    await RepublishWithRetryAsync(_channel, "order.status-changed", body, retryCount + 1, stoppingToken);
                }

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("notifications.order-status", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private static int GetRetryCount(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers == null || !properties.Headers.TryGetValue("x-retry", out var raw) || raw == null)
        {
            return 0;
        }

        if (raw is byte[] bytes && int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static async Task RepublishWithRetryAsync(IChannel channel, string routingKey, string body, int retry, CancellationToken ct)
    {
        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
            Headers = new Dictionary<string, object?>
            {
                ["x-retry"] = retry.ToString()
            }
        };

        if (JsonSerializer.Deserialize<Dictionary<string, object?>>(body) is { } payload)
        {
            if (payload.TryGetValue("CorrelationId", out var correlationId) && correlationId != null)
            {
                props.CorrelationId = correlationId.ToString();
                props.Headers["x-correlation-id"] = correlationId.ToString();
            }

            if (payload.TryGetValue("MessageId", out var messageId) && messageId != null)
            {
                props.MessageId = messageId.ToString();
            }
        }

        var bytes = Encoding.UTF8.GetBytes(body);
        await channel.BasicPublishAsync("raffle.events", routingKey, false, props, bytes, ct);
    }
}
