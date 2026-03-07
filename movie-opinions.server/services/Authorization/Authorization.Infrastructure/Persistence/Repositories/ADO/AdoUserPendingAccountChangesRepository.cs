using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserPendingAccountChangesRepository : RepositoryBase, IUserPendingAccountChangesRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserPendingAccountChangesRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserPendingAccountChangesRepository> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
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
