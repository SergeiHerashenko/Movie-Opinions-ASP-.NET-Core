using Authorization.Domain.Entities;
using Contracts.Interfaces;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserDeletionRepository : IBaseRepository<UserDeletion>
    {
        Task<UserDeletion?> GetUserDeletionsByIdAsync(Guid userId);

        Task<UserDeletion?> GetUserDeletionsByLoginAsync(string userLogin);
    }
}
