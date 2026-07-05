namespace NotificationService.Services
{
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(ILogger<CacheInvalidationService> logger)
        {
            _logger = logger;
        }

        public Task InvalidateAsync(string key)
        {
            _logger.LogInformation("Cache invalidated for key: {Key}", key);
            return Task.CompletedTask;
        }
    }
}
