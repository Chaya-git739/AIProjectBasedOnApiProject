using BffService.DTOs;

namespace BffService.Services;

public interface IAggregationService
{
    Task<Result<OrderDetailsDto, (StandardError Body, int StatusCode)>> GetOrderDetailsAsync(string orderId, string correlationId);
}

public class AggregationService : IAggregationService
{
    private readonly IOrderClient _orderClient;
    private readonly IProductClient _productClient;
    private readonly ILogger<AggregationService> _logger;

    public AggregationService(IOrderClient orderClient, IProductClient productClient, ILogger<AggregationService> logger)
    {
        _orderClient = orderClient;
        _productClient = productClient;
        _logger = logger;
    }

    public async Task<Result<OrderDetailsDto, (StandardError Body, int StatusCode)>> GetOrderDetailsAsync(string orderId, string correlationId)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var ct = cts.Token;

        if (string.IsNullOrWhiteSpace(orderId) || !int.TryParse(orderId, out _))
        {
            var err = new StandardError("INVALID_INPUT", "orderId must be a numeric string", correlationId, DateTime.UtcNow) { StatusCode = 400 };
            return Result<OrderDetailsDto, (StandardError, int)>.FromError((err, 400));
        }

        var orderResult = await _orderClient.GetOrderAsync(orderId, correlationId, ct);
        if (!orderResult.IsSuccess)
        {
            var status = orderResult.StatusCode;
            var err = status switch
            {
                404 => new StandardError("NOT_FOUND", "order not found", correlationId, DateTime.UtcNow) { StatusCode = 404 },
                401 => new StandardError("UNAUTHORIZED", "unauthorized to read order", correlationId, DateTime.UtcNow) { StatusCode = 401 },
                408 => new StandardError("UPSTREAM_TIMEOUT", "OrderService timed out", correlationId, DateTime.UtcNow) { StatusCode = 504 },
                _ => new StandardError("DEPENDENCY_FAILED", "OrderService is unavailable", correlationId, DateTime.UtcNow) { StatusCode = 424 }
            };
            return Result<OrderDetailsDto, (StandardError, int)>.FromError((err, err.StatusCode));
        }

        var order = orderResult.Payload!;
        var items = new List<OrderItemDto>();

        foreach (var orderItem in order.OrderItems)
        {
            var giftResult = await _productClient.GetGiftAsync(orderItem.GiftId, correlationId, ct);
            if (!giftResult.IsSuccess)
            {
                _logger.LogWarning("Gift {GiftId} fetch failed with status {StatusCode}", orderItem.GiftId, giftResult.StatusCode);
                var err = giftResult.StatusCode == 408
                    ? new StandardError("UPSTREAM_TIMEOUT", "ProductCatalog service timed out", correlationId, DateTime.UtcNow) { StatusCode = 504 }
                    : new StandardError("DEPENDENCY_FAILED", "ProductCatalog service failed", correlationId, DateTime.UtcNow) { StatusCode = 424 };
                return Result<OrderDetailsDto, (StandardError, int)>.FromError((err, err.StatusCode));
            }

            var gift = giftResult.Payload!;
            var unit = gift.TicketPrice;
            var line = unit * orderItem.Quantity;
            items.Add(new OrderItemDto(orderItem.GiftId, gift.Name, orderItem.Quantity, unit, line));
        }

        var subtotal = items.Sum(i => i.LineTotal);
        var tax = Math.Round(subtotal * 0.1m, 2);
        var total = subtotal + tax;

        var dto = new OrderDetailsDto(order.Id, new PurchaserDto(order.UserId, null, null), items, subtotal, tax, total, new MetaDto(correlationId));
        return Result<OrderDetailsDto, (StandardError, int)>.FromSuccess(dto);
    }
}
