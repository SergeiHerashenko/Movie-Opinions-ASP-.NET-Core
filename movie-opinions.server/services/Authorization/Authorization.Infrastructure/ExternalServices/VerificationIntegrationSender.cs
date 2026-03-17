using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Http;
using Contracts.Model.Response;
using Contracts.Models;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.ExternalServices
{
    public class VerificationIntegrationSender : IVerificationSender
    {
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly ILogger<VerificationIntegrationSender> _logger;

        public VerificationIntegrationSender(ISendInternalRequest sendInternalRequest,
            ILogger<VerificationIntegrationSender> logger)
        {
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
        }

        public async Task<ServiceResponse<string>> GetCode(Guid requestId)
        {
            var verificationRequest = new InternalRequest<object>()
            {
                ClientName = "VerificationClient",
                Endpoint = $"api/varification/get-code/{requestId}",
                Method = HttpMethod.Get,
            };

            return await ExecuteVerificationRequestAsync<object, string>(verificationRequest, requestId);
        }

        private async Task<ServiceResponse<TResponse>> ExecuteVerificationRequestAsync<TBody, TResponse>(
            InternalRequest<TBody> request,
            Guid requestId)
        {
            var responseVerification = await _sendInternalRequest.SendAsync<TBody, TResponse>(request);

            if (!responseVerification.IsSuccess)
            {
                _logger.LogError("Помилка отримання коду для запису {RequestId}. Статус: {StatusCode}",
                    requestId,
                    responseVerification.StatusCode);

                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    StatusCode = responseVerification.StatusCode,
                    Message = responseVerification.Message
                };
            }

            return new ServiceResponse<TResponse>
            {
                IsSuccess = true,
                StatusCode = responseVerification.StatusCode,
                Message = "Код успішно отримано",
                Data = responseVerification.Data
            };
        }
    }
}
