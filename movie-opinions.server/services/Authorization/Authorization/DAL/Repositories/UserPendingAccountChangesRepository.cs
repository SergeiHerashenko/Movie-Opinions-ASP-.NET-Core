using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Repositories
{
    public class UserPendingAccountChangesRepository : IUserPendingAccountChangesRepository
    {
        public Task<RepositoryResponse<UserPendingChanges>> CreateAsync(UserPendingChanges entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserPendingChanges>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserPendingChanges>> UpdateAsync(UserPendingChanges entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserPendingChanges>> GetPendingChangesAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
