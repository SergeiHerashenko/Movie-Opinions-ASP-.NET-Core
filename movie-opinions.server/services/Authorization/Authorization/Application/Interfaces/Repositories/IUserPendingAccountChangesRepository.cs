using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserPendingAccountChangesRepository : IBaseRepository<UserPendingChange, RepositoryResponse<UserPendingChange>>
    {
        Task<RepositoryResponse<UserPendingChange>> GetPendingChangesAsync(string token);
    }
}
