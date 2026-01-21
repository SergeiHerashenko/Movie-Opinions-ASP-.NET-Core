using Microsoft.AspNetCore.Mvc;
using Notification.Models.Notification;
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

        [HttpPost("notification")]
        public async Task<IActionResult> SendAsync(NotificationRequest notification)
        {
            var sendResult = await _notificationService.SendAsync(notification);
            return Ok(sendResult);
        }
    }
}
