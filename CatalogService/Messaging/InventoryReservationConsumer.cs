using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CatalogService.Services;

namespace CatalogService.Messaging;

public sealed class InventoryReservationConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InventoryReservationConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public InventoryReservationConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<InventoryReservationConsumer> logger)
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
            ["x-dead-letter-routing-key"] = "order.placed.dead"
        };

        await _channel.QueueDeclareAsync(
            queue: "inventory.order-placed",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync("inventory.order-placed", "raffle.events", "order.placed", cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync("inventory.order-placed.dlq", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("inventory.order-placed.dlq", "raffle.events", "order.placed.dead", cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync("inventory.release-requested", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync("inventory.release-requested", "raffle.events", "inventory.release-requested", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(body);

            if (evt == null)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IProcessedMessageStore>();
            if (!store.TryMarkProcessed(evt.MessageId))
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                return;
            }

            var retryCount = GetRetryCount(ea.BasicProperties);

            try
            {
                var giftService = scope.ServiceProvider.GetRequiredService<IGiftService>();
                var allAvailable = true;

                _logger.LogWarning("[Saga] Received OrderPlaced for OrderId={OrderId} CorrelationId={CorrelationId}", evt.OrderId, evt.CorrelationId);

                foreach (var item in evt.Items)
                {
                    var gift = await giftService.GetByIdAsync(item.GiftId);
                    if (gift == null)
                    {
                        allAvailable = false;
                        break;
                    }
                }

                if (allAvailable)
                {
                    var reserved = new InventoryReservedEvent
                    {
                        CorrelationId = evt.CorrelationId,
                        OrderId = evt.OrderId,
                        Success = true,
                        Reason = null
                    };

                    await PublishAsync(_channel, "inventory.reserved", reserved, stoppingToken);
                    _logger.LogWarning("[Saga] Published inventory.reserved for OrderId={OrderId}", evt.OrderId);
                }
                else
                {
                    var rejected = new InventoryRejectedEvent
                    {
                        CorrelationId = evt.CorrelationId,
                        OrderId = evt.OrderId,
                        Reason = "Missing gift(s) in catalog"
                    };

                    await PublishRejectedAsync(_channel, "inventory.rejected", rejected, stoppingToken);
                    _logger.LogWarning("[Saga] Published inventory.rejected for OrderId={OrderId} Reason={Reason}", evt.OrderId, rejected.Reason);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inventory consumer failed for order {OrderId} retry={Retry}", evt.OrderId, retryCount);

                if (retryCount < 3)
                {
                    await RepublishWithRetryAsync(_channel, "order.placed", body, retryCount + 1, stoppingToken);
                }

                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("inventory.order-placed", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        var releaseConsumer = new AsyncEventingBasicConsumer(_channel);
        releaseConsumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var releaseEvt = JsonSerializer.Deserialize<InventoryReleaseRequestedEvent>(body);
                if (releaseEvt != null)
                {
                    _logger.LogWarning("[Saga] Received inventory.release-requested for OrderId={OrderId} Items={ItemsCount}", releaseEvt.OrderId, releaseEvt.Items.Count);
                }
            }
            catch
            {
                // Keep release consumer tolerant because it is a best-effort compensation hook.
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync("inventory.release-requested", autoAck: false, consumer: releaseConsumer, cancellationToken: stoppingToken);

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

    private static async Task PublishAsync(IChannel channel, string routingKey, InventoryReservedEvent payload, CancellationToken ct)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(payload);
        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync("raffle.events", routingKey, false, props, data, ct);
    }

    private static async Task PublishRejectedAsync(IChannel channel, string routingKey, InventoryRejectedEvent payload, CancellationToken ct)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(payload);
        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync("raffle.events", routingKey, false, props, data, ct);
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

        var bytes = Encoding.UTF8.GetBytes(body);
        await channel.BasicPublishAsync("raffle.events", routingKey, false, props, bytes, ct);
    }
}
