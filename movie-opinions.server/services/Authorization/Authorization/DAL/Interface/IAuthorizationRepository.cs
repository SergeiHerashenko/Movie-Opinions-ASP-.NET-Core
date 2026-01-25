using Authorization.Models.User;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IAuthorizationRepository : IBaseRepository<UserEntity, RepositoryResponse<UserEntity>>
    {
        Task<RepositoryResponse<UserEntity>> GetUserByEmailAsync(string email);

        Task<RepositoryResponse<UserEntity>> GetUserByIdAsync(Guid userId);
    }
}
