using MovieOpinions.Contracts.Models.RepositoryResponse;
using Notification.Models.Notification;

namespace Notification.DAL.Interface
{
    public interface INotificationRepository
    {
        Task<RepositoryResponse<NotificationEntity>> SaveAsync(NotificationEntity entity);
    }
}
