using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IUserDeletionRepository : IBaseRepository<UserDeletion, RepositoryResponse<UserDeletion>>
    {
        Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByIdAsync(Guid idUser);
    }
}
