using Authorization.Domain.Entities;
using Contracts.Interfaces;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetUserByIdAsync(Guid userId);

        Task<User> GetUserByLoginAsync(string userLogin);

        Task<User?> FindUserByLoginAsync(string userLogin);
    }
}
