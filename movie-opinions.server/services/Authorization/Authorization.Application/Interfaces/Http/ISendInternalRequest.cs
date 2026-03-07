using Contracts.Model.Response;
using Contracts.Models;

namespace Authorization.Application.Interfaces.Http
{
    public interface ISendInternalRequest
    {
        Task<ServiceResponse<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest);
    }
}
