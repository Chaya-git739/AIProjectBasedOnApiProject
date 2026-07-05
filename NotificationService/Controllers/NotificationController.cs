using Microsoft.AspNetCore.Mvc;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] NotificationRequest request)
        {
            var result = await _notificationService.SendAsync(request.To, request.Subject, request.Message);
            return Ok(new { success = result });
        }
    }

    public class NotificationRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
