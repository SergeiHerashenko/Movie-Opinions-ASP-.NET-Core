using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IUserRepository : IBaseRepository<User, RepositoryResponse<User>>
    {
        Task<RepositoryResponse<User>> GetUserByIdAsync(Guid idUser);

        Task<RepositoryResponse<User>> GetUserByEmailAsync(string emailUser);
    }
}
