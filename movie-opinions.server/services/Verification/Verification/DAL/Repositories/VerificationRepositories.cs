using MovieOpinions.Contracts.Models.RepositoryResponse;
using Verification.DAL.Interface;
using Verification.Models;

namespace Verification.DAL.Repositories
{
    public class VerificationRepositories : IVerificationRepositories
    {
        public Task<RepositoryResponse<Guid>> Create(VerificationEntity verificationEntity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<Guid>> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<Guid>> Update(VerificationEntity verificationEntity)
        {
            throw new NotImplementedException();
        }
    }
}
