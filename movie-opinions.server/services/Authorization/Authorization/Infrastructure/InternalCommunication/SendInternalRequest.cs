using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models.ServiceResult;

namespace Authorization.Infrastructure.InternalCommunication
{
    public class SendInternalRequest : ISendInternalRequest
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SendInternalRequest(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ServiceResponse<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalReques)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(internalReques.ClientName);
                HttpResponseMessage response;

                if (internalReques.Method == HttpMethod.Post)
                {
                    response = await client.PostAsJsonAsync(internalReques.Endpoint, internalReques.Body);
                }
                else
                {
                    response = await client.GetAsync(internalReques.Endpoint);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TResponse>();

                    return new ServiceResponse<TResponse>
                    {
                        IsSuccess = true,
                        Data = result,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var errorData = await response.Content.ReadFromJsonAsync<ServiceResult<object>>();

                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = errorData?.Message ?? response.ReasonPhrase,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Критична помилка: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
