using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IUserPendingAccountChangesRepository : IBaseRepository<UserPendingChanges, RepositoryResponse<UserPendingChanges>>
    {
        Task<RepositoryResponse<UserPendingChanges>> GetPendingChangesAsync(string token);
    }
}
