namespace CatalogService.Messaging;

public sealed class OrderPlacedEvent
{
    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}

public sealed class OrderPlacedItemEvent
{
    public int GiftId { get; set; }
    public int Quantity { get; set; }
}
