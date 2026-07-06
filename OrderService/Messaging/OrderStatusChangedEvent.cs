namespace OrderService.Messaging;

public sealed class OrderStatusChangedEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
