using Microsoft.Extensions.Logging;
using Profile.Application.Interfaces.Repositories;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence.Context.AdoNet;
using Profile.Infrastructure.Persistence.Repositories.Base;

namespace Profile.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserProfileRepositories : RepositoryBase, IUserProfileRepositories
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserProfileRepositories(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserProfileRepositories> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public Task<UserProfile> CreateAsync(UserProfile entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile> UpdateAsync(UserProfile entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfile> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
