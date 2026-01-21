using MovieOpinions.Contracts.Models.ServiceResponse;
using Verification.DAL.Interface;
using Verification.Models;
using Verification.Services.Interfaces;

namespace Verification.Services.Implementations
{
    public class VerificationService : IVerificationService
    {
        private readonly IVerificationRepositories _verificationRepositories;

        public VerificationService(IVerificationRepositories verificationRepositories)
        {
            _verificationRepositories = verificationRepositories;
        }

        public Task<ServiceResponse<Guid>> ConfirmVerification()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<VerificationEntity>> GenerateVerificationToken()
        {
            // Заглушка
            var createToken = await _verificationRepositories.Create(new VerificationEntity());
            return new ServiceResponse<VerificationEntity>()
            {

            };
        }
    }
}
