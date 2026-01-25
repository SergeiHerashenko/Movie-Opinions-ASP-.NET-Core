using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Verification.DAL.Interface;
using Verification.Models;

namespace Verification.DAL.Repositories
{
    public class VerificationRepositories : IVerificationRepositories
    {
        public Task<RepositoryResponse<VerificationEntity>> CreateAsync(VerificationEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<VerificationEntity>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<Guid>> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<VerificationEntity>> UpdateAsync(VerificationEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
