namespace OrderService.Messaging;

public sealed class InventoryReleaseRequestedEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}
