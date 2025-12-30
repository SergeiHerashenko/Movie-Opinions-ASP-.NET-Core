using Authorization.Models.Responses;
using Authorization.Models.User;

namespace Authorization.DAL.Interface
{
    public interface IAuthorizationRepository
    {
        Task<RepositoryResult<UserEntityDTO>> CreateUserAsync(UserEntity userEntity);

        Task<RepositoryResult<bool>> DeleteUserAsync(Guid userId);

        Task<RepositoryResult<UserEntity>> GetUserByEmailAsync(string email);

        Task<RepositoryResult<UserEntity>> GetUserByIdAsync(Guid userId);
    }
}
