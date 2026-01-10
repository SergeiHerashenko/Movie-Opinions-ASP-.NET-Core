using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Send()
        {
            return View();
        }
    }
}
