using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserRepository : RepositoryBase, IUserRepository
    {
        public DapperUserRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<DapperUserRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public Task<User> CreateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByLoginAsync(Login login)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByLoginAsync(Login login)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
