using MailKit.Net.Smtp;
using MimeKit;

namespace NotificationService.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly IConfiguration _configuration;

        public EmailNotificationService(ILogger<EmailNotificationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendWinnerNotificationAsync(string email, string userName, string giftName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? string.Empty;
                var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? string.Empty;
                var senderName = _configuration["EmailSettings:SenderName"] ?? "מערכת ההגרלות";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(userName, email));
                message.Subject = "מזל טוב! זכית בהגרלה!";

                message.Body = new TextPart("html")
                {
                    Text = $@"
                        <div dir='rtl' style='font-family: Arial, sans-serif;'>
                            <h2>שלום {userName},</h2>
                            <p><strong>מזל טוב! זכית בהגרלה!</strong></p>
                            <p>אנו שמחים לבשר לך שזכית במתנה: <strong>{giftName}</strong></p>
                            <p>אנא צור קשר איתנו לתיאום איסוף המתנה.</p>
                            <p>ברכות,<br/>צוות מערכת ההגרלות</p>
                        </div>"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בשליחת מייל זכייה ל-{Email}", email);
                throw;
            }
        }
    }
}
