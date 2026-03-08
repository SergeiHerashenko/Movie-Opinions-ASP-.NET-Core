using Authorization.Application.DTO.Integration;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Http;
using Contracts.Model.Response;
using Contracts.Models;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.ExternalServices
{
    public class ProfileIntegrationSender : IProfileSender
    {
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly ILogger<ProfileIntegrationSender> _logger;

        public ProfileIntegrationSender(ISendInternalRequest sendInternalRequest,
            ILogger<ProfileIntegrationSender> logger)
        {
            _sendInternalRequest = sendInternalRequest;
            _logger = logger;
        }

        public async Task<ServiceResponse> SendCreateProfileRequestAsync(ProfileIntegrationDTO profileIntegrationDTO)
        {
            var profileRequest = new InternalRequest<ProfileIntegrationDTO>
            {
                ClientName = "ProfileClient",
                Endpoint = "api/profile/create",
                Method = HttpMethod.Post,
                Body = profileIntegrationDTO
            };
            
            return await ExecuteProfileRequestAsync(profileRequest, profileIntegrationDTO.UserId, "Створення");
        }

        public async Task<ServiceResponse> SendUpdateProfileRequestAsync(ProfileIntegrationDTO profileIntegrationDTO)
        {
            var profileRequest = new InternalRequest<ProfileIntegrationDTO>
            {
                ClientName = "ProfileClient",
                Endpoint = "api/profile/update",
                Method = HttpMethod.Put,
                Body = profileIntegrationDTO
            };

            return await ExecuteProfileRequestAsync(profileRequest, profileIntegrationDTO.UserId, "Оновлення");
        }

        public async Task<ServiceResponse> SendDeleteProfileRequestAsync(Guid userId)
        {
            var profileRequest = new InternalRequest<object>
            {
                ClientName = "ProfileClient",
                Endpoint = $"api/profile/delete/{userId}",
                Method = HttpMethod.Delete
            };

            return await ExecuteProfileRequestAsync(profileRequest, userId, "Видалення");
        }

        private async Task<ServiceResponse> ExecuteProfileRequestAsync<TBody>(
            InternalRequest<TBody> request,
            Guid userId,
            string actionName
            )
        {
            var responseProfile = await _sendInternalRequest.SendAsync<TBody, object>(request);

            if (!responseProfile.IsSuccess)
            {
                _logger.LogError("Помилка інтеграції ({Action}) для користувача {UserId}. Статус: {StatusCode}",
                    actionName,
                    userId, 
                    responseProfile.StatusCode);

                return new ServiceResponse
                {
                    IsSuccess = false,
                    StatusCode = responseProfile.StatusCode,
                    Message = responseProfile.Message
                };
            }

            return new ServiceResponse
            {
                IsSuccess = true,
                StatusCode = responseProfile.StatusCode,
                Message = $"Операція {actionName} успішна!"
            };
        }
    }
}
