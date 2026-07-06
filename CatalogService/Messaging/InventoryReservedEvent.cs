namespace CatalogService.Messaging;

public sealed class InventoryReservedEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public bool Success { get; set; }
    public string? Reason { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class InventoryRejectedEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class InventoryReleaseRequestedEvent
{
    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}
