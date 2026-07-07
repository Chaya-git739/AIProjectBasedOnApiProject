using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Services;

namespace OrderService.Messaging;

public sealed class InventoryReservedConsumer : BackgroundService
{
    private const string QueueName = "orders.inventory-reserved.v2";
    private const string DlqName = "orders.inventory-reserved.v2.dlq";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InventoryReservedConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public InventoryReservedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<InventoryReservedConsumer> logger)
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
            ["x-dead-letter-routing-key"] = "inventory.reserved.dead"
        };

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(QueueName, "raffle.events", "inventory.reserved", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, "raffle.events", "inventory.rejected", cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(DlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(DlqName, "raffle.events", "inventory.reserved.dead", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var reservedEvt = JsonSerializer.Deserialize<InventoryReservedEvent>(body);
            var rejectedEvt = reservedEvt == null ? JsonSerializer.Deserialize<InventoryRejectedEvent>(body) : null;
            var evt = reservedEvt ?? (rejectedEvt == null
                ? null
                : new InventoryReservedEvent
                {
                    MessageId = rejectedEvt.MessageId,
                    CorrelationId = rejectedEvt.CorrelationId,
                    OrderId = rejectedEvt.OrderId,
                    Success = false,
                    Reason = rejectedEvt.Reason,
                    ProcessedAtUtc = rejectedEvt.ProcessedAtUtc
                });

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
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderApplicationService>();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                _logger.LogWarning("[Saga] Received inventory result for OrderId={OrderId} Success={Success} CorrelationId={CorrelationId}", evt.OrderId, evt.Success, evt.CorrelationId);

                if (evt.Success)
                {
                    await orderService.ConfirmOrderAsync(evt.OrderId);
                    _logger.LogWarning("[Saga] Order confirmed for OrderId={OrderId}", evt.OrderId);
                }
                else
                {
                    await orderService.CancelOrderAsync(evt.OrderId);
                    _logger.LogWarning("[Saga] Order canceled for OrderId={OrderId} Reason={Reason}", evt.OrderId, evt.Reason);

                    var orderForComp = await orderService.GetOrderByIdAsync(evt.OrderId);
                    var release = new InventoryReleaseRequestedEvent
                    {
                        CorrelationId = evt.CorrelationId,
                        OrderId = evt.OrderId,
                        Items = orderForComp?.OrderItems.Select(i => new OrderPlacedItemEvent
                        {
                            GiftId = i.GiftId,
                            Quantity = i.Quantity
                        }).ToList() ?? new List<OrderPlacedItemEvent>()
                    };

                    await publisher.PublishAsync("inventory.release-requested", release, stoppingToken);
                    _logger.LogWarning("[Saga] Published inventory.release-requested for OrderId={OrderId}", evt.OrderId);
                }

                var order = await orderService.GetOrderByIdAsync(evt.OrderId);
                var userId = order?.UserId ?? 0;

                var statusEvent = new OrderStatusChangedEvent
                {
                    CorrelationId = evt.CorrelationId,
                    OrderId = evt.OrderId,
                    UserId = userId,
                    Status = evt.Success ? "Confirmed" : "Rejected",
                    Reason = evt.Reason
                };

                await publisher.PublishAsync("order.status-changed", statusEvent, stoppingToken);
                _logger.LogWarning("[Saga] Published order.status-changed for OrderId={OrderId} Status={Status}", evt.OrderId, statusEvent.Status);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed handling inventory event for order {OrderId} retry={Retry}", evt.OrderId, retryCount);

                if (retryCount < 3)
                {
                    await RepublishWithRetryAsync(_channel, "inventory.reserved", body, retryCount + 1, stoppingToken);
                }

                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

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

        if (JsonSerializer.Deserialize<Dictionary<string, object?>>(body) is { } payload &&
            payload.TryGetValue("CorrelationId", out var correlationId) &&
            correlationId != null)
        {
            props.CorrelationId = correlationId.ToString();
            props.Headers["x-correlation-id"] = correlationId.ToString();
        }

        if (JsonSerializer.Deserialize<Dictionary<string, object?>>(body) is { } retryPayload &&
            retryPayload.TryGetValue("MessageId", out var messageId) &&
            messageId != null)
        {
            props.MessageId = messageId.ToString();
        }

        var bytes = Encoding.UTF8.GetBytes(body);
        await channel.BasicPublishAsync("raffle.events", routingKey, false, props, bytes, ct);
    }
}
