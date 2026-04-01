using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByLoginAsync(Login login);

        Task<User?> GetUserByIdAsync(Guid userId);

        Task<bool> ExistsByLoginAsync(Login login);
    }
}
