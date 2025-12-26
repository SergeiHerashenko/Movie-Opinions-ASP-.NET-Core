using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.DAL.Interface
{
    public interface IAuthRepository
    {
        Task<RepositoryResult<UserEntityDTO>> CreateUserAsync(UserEntity userEntity);

        Task<RepositoryResult<bool>> DeleteUserAsync(Guid userId);

        Task<RepositoryResult<UserEntity>> GetUserByEmailAsync(string email);

        Task<RepositoryResult<UserEntity>> GetUserByIdAsync(Guid userId);
    }
}
