using MovieOpinions.Contracts.Models.ServiceResponse;
using Verification.Models;

namespace Verification.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<ServiceResponse<string>> GenerateVerificationToken(Guid userId);

        Task<ServiceResponse<VerificationEntity>> GenerateVerificationCode();

        Task<ServiceResponse<Guid>> ConfirmVerification();
    }
}
