namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        public Task<bool> SendAsync(string to, string subject, string message)
        {
            // Placeholder for the first extraction slice.
            // Keeps the service isolated and safe while the old app remains unchanged.
            Console.WriteLine($"Notification sent to {to}: {subject}");
            return Task.FromResult(true);
        }
    }
}
