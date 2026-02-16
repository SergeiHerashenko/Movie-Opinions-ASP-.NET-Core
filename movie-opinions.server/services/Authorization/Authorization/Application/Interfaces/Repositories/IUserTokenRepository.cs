using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserTokenRepository : IBaseRepository<UserToken, RepositoryResponse<UserToken>>
    {
        Task<RepositoryResponse<UserToken>> GetUserTokenAsync(string refreshToken);
    }
}
