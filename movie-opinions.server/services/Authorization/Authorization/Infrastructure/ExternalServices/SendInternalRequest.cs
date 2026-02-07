using Authorization.Application.Interfaces.Integration;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResult;
using XAct.Messages;

namespace Authorization.Infrastructure.ExternalServices
{
    public class SendInternalRequest : ISendInternalRequest
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SendInternalRequest> _logger;

        public SendInternalRequest(IHttpClientFactory httpClientFactory, ILogger<SendInternalRequest> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ServiceResult<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(internalRequest.ClientName);

                var response = internalRequest.Method.Method switch
                {
                    "POST" => await client.PostAsJsonAsync(internalRequest.Endpoint, internalRequest.Body),
                    "GET" => await client.GetAsync(internalRequest.Endpoint),
                    _ => throw new NotSupportedException()
                };

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<TResponse>>();

                if (response.IsSuccessStatusCode && result != null)
                {
                    return result;
                }

                return result ?? new ServiceResult<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Сервіс повернув помилку: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка зв'язку з {Client}", internalRequest.ClientName);

                return new ServiceResult<TResponse>
                {
                    IsSuccess = false,
                    Message = "Зовнішній сервіс недоступний",
                    StatusCode = StatusCode.General.InternalError
                };
            }
        }
    }
}