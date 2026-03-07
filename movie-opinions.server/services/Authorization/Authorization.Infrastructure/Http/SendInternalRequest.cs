using Authorization.Application.Interfaces.Http;
using Contracts.Model.Response;
using Contracts.Models;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Authorization.Infrastructure.Http
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

        public async Task<ServiceResponse<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(internalRequest.ClientName);

                // 1. Відправка запиту
                HttpResponseMessage response = internalRequest.Method.Method.ToUpper() switch
                {
                    "POST" => await client.PostAsJsonAsync(internalRequest.Endpoint, internalRequest.Body),
                    "GET" => await client.GetAsync(internalRequest.Endpoint),
                    "PUT" => await client.PutAsJsonAsync(internalRequest.Endpoint, internalRequest.Body),
                    "DELETE" => await client.DeleteAsync(internalRequest.Endpoint),
                    _ => throw new NotSupportedException($"Метод {internalRequest.Method} не підтримується")
                };

                // 2. Спроба десеріалізувати відповідь
                // Оскільки мідлваре завжди повертає ServiceResponse<T>, очікуємо саме його
                var result = await response.Content.ReadFromJsonAsync<ServiceResponse<TResponse>>();

                if (result != null)
                {
                    // Якщо HTTP статус успішний — повертаємо результат як є
                    if (response.IsSuccessStatusCode)
                    {
                        return result;
                    }

                    // Якщо статус не успішний (наприклад 400), але сервіс повернув опис помилки в нашому форматі
                    _logger.LogWarning("Внутрішній запит до {Client} повернув помилку: {Message}",
                        internalRequest.ClientName, result.Message);

                    return result;
                }

                // 3. Якщо JSON не вдалося прочитати (наприклад, сервіс впав з 500 і віддав HTML чи пусту строку)
                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Помилка сервісу {internalRequest.ClientName}: {response.ReasonPhrase}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Мережева помилка при зверненні до {Client}", internalRequest.ClientName);
                return CreateErrorResult<TResponse>("Мережева помилка або сервіс офлайн");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непередбачена помилка у SendInternalRequest для {Client}", internalRequest.ClientName);
                return CreateErrorResult<TResponse>("Помилка обробки внутрішнього запиту");
            }
        }

        private ServiceResponse<T> CreateErrorResult<T>(string message)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = StatusCode.General.InternalError
            };
        }
    }
}
