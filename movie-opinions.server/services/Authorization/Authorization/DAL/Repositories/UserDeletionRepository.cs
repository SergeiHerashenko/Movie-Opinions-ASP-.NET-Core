using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Repositories
{
    public class UserDeletionRepository : IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _dbConnection;

        public UserDeletionRepository(IDbConnectionProvider dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Task<RepositoryResponse<UserDeletion>> CreateAsync(UserDeletion entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserDeletion>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserDeletion>> UpdateAsync(UserDeletion entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByIdAsync(Guid idUser)
        {
            throw new NotImplementedException();
        }
    }
}
