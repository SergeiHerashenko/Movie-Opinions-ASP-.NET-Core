using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.DAL.Interface
{
    public interface IAuthRepository
    {
        Task<RepositoryResult<UserEntityDTO>> RegistrationUserAsync(UserEntity userEntity);
        
        Task<RepositoryResult<UserEntity>> GetUserByEmailAsync(string email);

        Task<RepositoryResult<bool>> DeleteUserAsync(Guid userId);
    }
}
