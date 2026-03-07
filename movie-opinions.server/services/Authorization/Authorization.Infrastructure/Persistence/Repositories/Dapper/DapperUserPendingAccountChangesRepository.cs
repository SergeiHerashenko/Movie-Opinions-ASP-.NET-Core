using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserPendingAccountChangesRepository : RepositoryBase, IUserPendingAccountChangesRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public DapperUserPendingAccountChangesRepository(
            IDbConnectionProvider connectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public Task<UserPendingChange> CreateAsync(UserPendingChange entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingChange> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingChange> UpdateAsync(UserPendingChange entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingChange> GetPendingChangesAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
