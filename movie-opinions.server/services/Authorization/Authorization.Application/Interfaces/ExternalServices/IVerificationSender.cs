using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface IVerificationSender
    {
        Task<ServiceResponse<string>> GetCode(Guid requestId);
    }
}
