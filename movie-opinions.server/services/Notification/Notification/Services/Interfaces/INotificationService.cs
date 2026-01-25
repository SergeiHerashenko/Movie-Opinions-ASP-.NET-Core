using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models;
using Notification.Models.Request;

namespace Notification.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResponse<StatusCode>> SendAsync (NotificationRequest notification);
    }
}
