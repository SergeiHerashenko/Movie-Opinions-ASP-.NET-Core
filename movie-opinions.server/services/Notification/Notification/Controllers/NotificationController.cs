using Microsoft.AspNetCore.Mvc;
using Notification.Models.Request;
using Notification.Services.Interfaces;

namespace Notification.Controllers
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
        public async Task<IActionResult> SendAsync([FromBody] NotificationRequest notification)
        {
            var sendResult = await _notificationService.SendAsync(notification);
            return Ok(sendResult);
        }
    }
}
