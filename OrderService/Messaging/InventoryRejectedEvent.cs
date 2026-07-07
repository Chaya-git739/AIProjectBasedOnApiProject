namespace OrderService.Messaging;

public sealed class InventoryRejectedEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
