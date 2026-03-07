using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserDeletionRepository : RepositoryBase, IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public DapperUserDeletionRepository(
            IDbConnectionProvider connectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public Task<UserDeletion> CreateAsync(UserDeletion entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> UpdateAsync(UserDeletion entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> GetUserDeletionsByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> GetUserDeletionsByLoginAsync(string userLogin)
        {
            throw new NotImplementedException();
        }
    }
}
