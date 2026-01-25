using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Notification.Models.Notification;

namespace Notification.DAL.Interface
{
    public interface INotificationRepository : IBaseRepository<NotificationEntity, RepositoryResponse<NotificationEntity>>
    {

    }
}
