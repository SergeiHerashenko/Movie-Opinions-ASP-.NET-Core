using MovieOpinions.Contracts.Models.RepositoryResponse;
using Notification.DAL.Interface;
using Notification.Models.Notification;

namespace Notification.DAL.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        public Task<RepositoryResponse<NotificationEntity>> SaveAsync(NotificationEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
