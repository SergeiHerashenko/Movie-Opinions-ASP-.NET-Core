using Authorization.Domain.Entities;
using Authorization.Models.User;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IAuthorizationRepository : IBaseRepository<User, RepositoryResponse<User>>
    {
        Task<RepositoryResponse<User>> GetUserByEmailAsync(string email);

        Task<RepositoryResponse<User>> GetUserByIdAsync(Guid userId);

        Task<RepositoryResponse<UserTokenEntity>> CreateTokenAsync(UserTokenEntity tokenEntity);

        Task<RepositoryResponse<UserTokenEntity>> GetTokenAsync(string refreshToken, Guid idUser);
    }
}
