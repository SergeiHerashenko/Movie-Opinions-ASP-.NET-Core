using MovieOpinions.Contracts.Models.RepositoryResponse;
using Notification.DAL.Interface;
using Notification.Models.Notification;

namespace Notification.DAL.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        public Task<RepositoryResponse<NotificationEntity>> CreateAsync(NotificationEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<NotificationEntity>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<NotificationEntity>> UpdateAsync(NotificationEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
