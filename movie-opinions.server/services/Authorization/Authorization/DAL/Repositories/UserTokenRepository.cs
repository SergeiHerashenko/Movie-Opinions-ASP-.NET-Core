using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly IDbConnectionProvider _dbConnection;

        public UserTokenRepository(IDbConnectionProvider dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Task<RepositoryResponse<UserToken>> CreateAsync(UserToken entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserToken>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserToken>> UpdateAsync(UserToken entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserToken>> GetUserTokenAsync()
        {
            throw new NotImplementedException();
        }
    }
}
