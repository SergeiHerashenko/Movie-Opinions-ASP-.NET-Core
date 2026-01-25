using Notification.Models.Enum;
using Notification.Models.Notification;
using Notification.Models.Responses.SenderResponses;
using Notification.Services.Interfaces;

namespace Notification.Services.Senders
{
    public class SmsSender : ISender
    {
        public NotificationChannel Channel => NotificationChannel.SMS;

        public async Task<SenderResponses> SendAsync(string destination, NotificationContent message)
        {
            return new SenderResponses
            {

            };
        }
    }
}
