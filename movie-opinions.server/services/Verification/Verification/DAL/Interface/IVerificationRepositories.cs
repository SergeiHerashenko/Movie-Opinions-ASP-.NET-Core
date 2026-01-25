using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Verification.Models;

namespace Verification.DAL.Interface
{
    public interface IVerificationRepositories : IBaseRepository<VerificationEntity, RepositoryResponse<VerificationEntity>>
    {
        Task<RepositoryResponse<Guid>> Get(Guid id);
    }
}
