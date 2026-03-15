using Authorization.Domain.Entities;
using Contracts.Interfaces;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserPendingAccountChangesRepository : IBaseRepository<UserPendingChange>
    {
        Task<UserPendingChange?> GetPendingChangesAsync(Guid id);
    }
}
