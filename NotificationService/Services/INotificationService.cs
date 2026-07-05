namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task<bool> SendAsync(string to, string subject, string message);
    }
}
