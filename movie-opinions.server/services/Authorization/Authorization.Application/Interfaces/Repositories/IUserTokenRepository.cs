using Authorization.Domain.Entities;
using Contracts.Interfaces;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserTokenRepository : IBaseRepository<UserToken>
    {
        Task<UserToken> GetUserTokenAsync(string refreshToken);

        Task<IEnumerable<UserToken>> GetAllTokensUserAsync(Guid userId);
    }
}
