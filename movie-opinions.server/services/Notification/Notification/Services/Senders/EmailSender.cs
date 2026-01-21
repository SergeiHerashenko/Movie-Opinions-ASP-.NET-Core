using Notification.Models.Enum;
using Notification.Models.Notification;
using Notification.Models.Responses.SenderResponses;
using Notification.Services.Interfaces;

namespace Notification.Services.Senders
{
    public class EmailSender : ISender
    {
        public NotificationChannel Channel => NotificationChannel.Email;

        public async Task<SenderResponses> SendAsync(string destination, NotificationMessage message)
        {
            return new SenderResponses
            {

            };
        }
    }
}
