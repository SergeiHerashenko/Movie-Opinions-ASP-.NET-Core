using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models;
using Notification.DAL.Interface;
using Notification.Models.Notification;
using Notification.Services.Interfaces;

namespace Notification.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<ISender> _senders;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(IEnumerable<ISender> senders, INotificationRepository notificationRepository)
        {
            _senders = senders;
            _notificationRepository = notificationRepository;
        }

        public async Task<ServiceResponse<StatusCode>> SendAsync(NotificationRequest notification)
        {
            var sender = _senders.FirstOrDefault(s => s.Channel == notification.Channel);

            if(sender == null)
            {
                var errorSender = new NotificationEntity()
                {
                    Id = Guid.NewGuid(),
                    Destination = notification.Destination,
                    Channel = notification.Channel,
                    Title = notification.Message.Title,
                    Body = notification.Message.Body,
                    Footer = notification.Message.Footer,
                    Status = Models.Enum.NotificationStatus.Failed,
                    CreatedAt = DateTime.UtcNow,
                    ErrorMessage = "Неіснуючий спосіб сповіщення!"
                };

                var saveErrorSender = await _notificationRepository.SaveAsync(errorSender);

                return new ServiceResponse<StatusCode>()
                {
                    IsSuccess = false,
                    StatusCode = saveErrorSender.StatusCode,
                    Message = saveErrorSender.Message
                };
            }

            try
            {
                // Виклик сервісу Template
                await sender.SendAsync(notification.Destination, notification.Message);
                
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StatusCode>()
                {

                };
            }
        }
    }
}
