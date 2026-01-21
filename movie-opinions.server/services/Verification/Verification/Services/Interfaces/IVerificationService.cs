using MovieOpinions.Contracts.Models.ServiceResponse;
using Verification.Models;

namespace Verification.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<ServiceResponse<VerificationEntity>> GenerateVerificationToken();

        Task<ServiceResponse<Guid>> ConfirmVerification();
    }
}
