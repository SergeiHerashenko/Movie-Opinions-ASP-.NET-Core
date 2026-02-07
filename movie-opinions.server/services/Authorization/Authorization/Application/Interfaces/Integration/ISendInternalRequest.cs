using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResult;

namespace Authorization.Application.Interfaces.Integration
{
    public interface ISendInternalRequest
    {
        Task<ServiceResult<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalRequest);
    }
}
