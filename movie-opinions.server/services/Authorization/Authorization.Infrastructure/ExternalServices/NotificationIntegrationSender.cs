using Authorization.Application.DTO.Integration;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Http;
using Contracts.Model.Response;
using Contracts.Models;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.ExternalServices
{
    public class NotificationIntegrationSender : INotificationSender
    {
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly ILogger<ProfileIntegrationSender> _logger;

        public NotificationIntegrationSender(ISendInternalRequest sendInternalRequest, ILogger<ProfileIntegrationSender> logger)
        {
            _sendInternalRequest = sendInternalRequest;
            _logger = logger;
        }

        public async Task<ServiceResponse> SendCreateNotificationAsync(NotificationIntegrationDTO notificationCreateIntegrationDTO)
        {
            var notificationRequest = new InternalRequest<NotificationIntegrationDTO>
            {
                ClientName = "NotificationClient",
                Endpoint = "api/notification/create",
                Method = HttpMethod.Post,
                Body = notificationCreateIntegrationDTO 
            };

            var responseNotification = await _sendInternalRequest.SendAsync<NotificationIntegrationDTO, bool>(notificationRequest);

            if (!responseNotification.IsSuccess)
            {
                _logger.LogError("Помилка відпрвлення листа підтвердження");

                return new ServiceResponse
                {
                    IsSuccess = false,
                    StatusCode = responseNotification.StatusCode,
                    Message = responseNotification.Message
                };
            }

            return new ServiceResponse
            {
                IsSuccess = true,
                StatusCode = responseNotification.StatusCode,
                Message = $"Операція відправки сповіщення успішна!"
            };
        }
    }
}
