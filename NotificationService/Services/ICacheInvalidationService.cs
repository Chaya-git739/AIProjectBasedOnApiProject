namespace NotificationService.Services
{
    public interface ICacheInvalidationService
    {
        Task InvalidateAsync(string key);
    }
}
