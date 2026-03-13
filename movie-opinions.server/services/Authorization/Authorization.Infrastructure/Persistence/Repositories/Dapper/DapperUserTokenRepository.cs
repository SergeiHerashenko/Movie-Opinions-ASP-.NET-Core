using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserTokenRepository : RepositoryBase, IUserTokenRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public DapperUserTokenRepository(
            IDbConnectionProvider connectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public Task<UserToken> CreateAsync(UserToken entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserToken> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserToken> UpdateAsync(UserToken entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserToken?> GetUserTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserToken>> GetAllTokensUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
