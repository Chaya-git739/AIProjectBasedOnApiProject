using System.Collections.Concurrent;

namespace NotificationService.Messaging;

public interface IProcessedMessageStore
{
    bool TryMarkProcessed(string messageId);
}

public sealed class ProcessedMessageStore : IProcessedMessageStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new();

    public bool TryMarkProcessed(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return false;
        }

        return _processed.TryAdd(messageId, 0);
    }
}
