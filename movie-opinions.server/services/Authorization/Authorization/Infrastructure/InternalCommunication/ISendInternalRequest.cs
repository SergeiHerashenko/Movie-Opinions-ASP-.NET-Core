using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Infrastructure.InternalCommunication
{
    public interface ISendInternalRequest
    {
        Task<ServiceResponse<TResponse>> SendAsync<TBody, TResponse>(InternalRequest<TBody> internalReques);
    }
}
