namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEmailNotificationService emailService, ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string message)
        {
            try
            {
                await _emailService.SendWinnerNotificationAsync(to, to, message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to {To}", to);
                return false;
            }
        }
    }
}
