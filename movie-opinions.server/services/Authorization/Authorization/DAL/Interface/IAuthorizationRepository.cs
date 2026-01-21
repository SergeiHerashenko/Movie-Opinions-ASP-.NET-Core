using Authorization.Models.User;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IAuthorizationRepository
    {
        Task<RepositoryResponse<UserEntityDTO>> CreateUserAsync(UserEntity userEntity);

        Task<RepositoryResponse<Guid>> DeleteUserAsync(Guid userId);

        Task<RepositoryResponse<UserEntity>> GetUserByEmailAsync(string email);

        Task<RepositoryResponse<UserEntity>> GetUserByIdAsync(Guid userId);
    }
}
