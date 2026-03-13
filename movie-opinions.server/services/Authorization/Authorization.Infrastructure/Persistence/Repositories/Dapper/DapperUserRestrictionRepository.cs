using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public DapperUserRestrictionRepository(
            IDbConnectionProvider connectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public Task<UserRestriction> CreateAsync(UserRestriction entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction> UpdateAsync(UserRestriction entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserRestriction?> GetBanByIdAsync(Guid banId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            throw new NotImplementedException();
        }
    }
}
