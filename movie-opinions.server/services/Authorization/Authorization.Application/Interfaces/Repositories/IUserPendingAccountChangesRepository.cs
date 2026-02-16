using Authorization.Domain.Entities;
using Contracts.Interface;
using Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserPendingAccountChangesRepository : IBaseRepository<UserPendingChange, RepositoryResponse<UserPendingChange>>
    {
        Task<RepositoryResponse<UserPendingChange>> GetPendingChangesAsync(string token);
    }
}
