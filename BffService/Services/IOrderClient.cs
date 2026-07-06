namespace BffService.Services;

public interface IOrderClient
{
    Task<HttpClientResult<OrderServiceContract.OrderDto>> GetOrderAsync(string orderId, string correlationId, CancellationToken ct);
}
