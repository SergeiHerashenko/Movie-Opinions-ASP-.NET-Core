using Contracts.Model.Response;

namespace Authorization.Application.Interfaces.ExternalServices
{
    public interface IVerificationSender
    {
        Task<ServiceResponse<string>> GetCodeAsync(Guid requestId);

        Task<ServiceResponse> UpdateAsync(Guid requestId, bool isConfirm);
    }
}
