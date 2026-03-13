using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Persistence.Repositories.Dapper
{
    public class DapperUserRepository : RepositoryBase, IUserRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public DapperUserRepository(
            IDbConnectionProvider connectionProvider,
            ILogger<DapperUserRepository> logger) 
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public Task<User> CreateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User?> FindUserByLoginAsync(string userLogin)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByLoginAsync(string userLogin)
        {
            throw new NotImplementedException();
        }
    }
}
