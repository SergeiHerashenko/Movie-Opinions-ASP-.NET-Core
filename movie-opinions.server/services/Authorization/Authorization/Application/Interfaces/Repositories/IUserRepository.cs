using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User, RepositoryResponse<User>>
    {
        Task<RepositoryResponse<User>> GetUserByIdAsync(Guid userId);

        Task<RepositoryResponse<User>> GetUserByLoginAsync(string userLogin);
    }
}
