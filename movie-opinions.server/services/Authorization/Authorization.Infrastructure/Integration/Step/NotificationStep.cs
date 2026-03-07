using Authorization.Application.DTO.Context;
using Authorization.Application.DTO.Integration;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Integration;
using Contracts.Integration;
using Contracts.Model.Response;

namespace Authorization.Infrastructure.Integration.Step
{
    public class NotificationStep(INotificationSender notificationSender) : IPostRegistrationStep
    {
        public int Order => int.MaxValue;

        public async Task<ServiceResponse> ExecuteAsync(RegistrationContext context)
        {
            var result = await notificationSender.SendCreateNotificationAsync(new NotificationIntegrationDTO
            {
                UserId = context.UserId,
                Recipient = context.Login,
                Action = context.Action,
                Channel = context.Channel
            });

            if (!result.IsSuccess)
            {
                if (context.Channel == CommunicationChannel.Phone)
                {
                    return result;
                }

                return new ServiceResponse
                {
                    IsSuccess = true,
                    StatusCode = result.StatusCode,
                    Message = "Сервіс сповіщень не працює. Спробуйте надіслати повторний лист в налаштуваннях профілю!"
                };
            }

            return result;
        }

        public Task RollbackAsync(Guid userId)
        {
            return Task.CompletedTask;
        }
    }
}
