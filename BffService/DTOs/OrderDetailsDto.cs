namespace BffService.DTOs;

public record OrderItemDto(int GiftId, string? Name, int Quantity, decimal UnitPrice, decimal LineTotal);

public record PurchaserDto(int UserId, string? Name, string? Email);

public record OrderDetailsDto
(
    int OrderId,
    PurchaserDto Purchaser,
    List<OrderItemDto> Items,
    decimal SubTotal,
    decimal Tax,
    decimal Total,
    MetaDto Meta
);

public record MetaDto(string CorrelationId, string Version = "v1");
