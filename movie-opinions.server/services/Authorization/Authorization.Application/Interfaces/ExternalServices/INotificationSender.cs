using Authorization.Application.DTO.Integration;
using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface INotificationSender
    {
        Task<ServiceResponse> SendCreateNotificationAsync(NotificationIntegrationDTO notificationCreateIntegrationDTO);
    }
}
