namespace NotificationService.Services
{
    public interface IEmailNotificationService
    {
        Task SendWinnerNotificationAsync(string email, string userName, string giftName);
    }
}
