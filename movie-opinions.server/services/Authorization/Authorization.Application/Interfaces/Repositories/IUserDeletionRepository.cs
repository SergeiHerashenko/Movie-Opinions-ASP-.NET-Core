using Authorization.Domain.Entities;
using Contracts.Interface;
using Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserDeletionRepository : IBaseRepository<UserDeletion, RepositoryResponse<UserDeletion>>
    {
        Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByIdAsync(Guid userId);

        Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByLoginAsync(string userLogin);
    }
}
