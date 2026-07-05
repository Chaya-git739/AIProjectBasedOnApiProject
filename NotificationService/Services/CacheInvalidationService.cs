using StackExchange.Redis;

namespace NotificationService.Services
{
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly IDatabase _db;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(IConnectionMultiplexer multiplexer, ILogger<CacheInvalidationService> logger)
        {
            _db = multiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task InvalidateAsync(string key)
        {
            var deleted = await _db.KeyDeleteAsync(key);
            if (deleted)
                _logger.LogInformation("Cache key deleted: {Key}", key);
            else
                _logger.LogWarning("Cache key not found, nothing deleted: {Key}", key);
        }
    }
}
