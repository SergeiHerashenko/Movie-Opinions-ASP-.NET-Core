using MovieOpinions.Contracts.Models.RepositoryResponse;
using Verification.Models;

namespace Verification.DAL.Interface
{
    public interface IVerificationRepositories
    {
        Task<RepositoryResponse<Guid>> Create(VerificationEntity verificationEntity);

        Task<RepositoryResponse<Guid>> Update(VerificationEntity verificationEntity);

        Task<RepositoryResponse<Guid>> Delete(Guid id);

        Task<RepositoryResponse<Guid>> Get(Guid id);
    }
}
