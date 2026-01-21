using Notification.Models.Enum;
using Notification.Models.Notification;
using Notification.Models.Responses.SenderResponses;

namespace Notification.Services.Interfaces
{
    public interface ISender
    {
        NotificationChannel Channel { get; }

        Task<SenderResponses> SendAsync (string destination, NotificationMessage message);
    }
}
