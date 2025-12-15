using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.DAL.Interface
{
    public interface IAuthRepository
    {
        Task<RepositoryResult<Guid>> CreateUser();
        // Змінити повернення модделі (додати нову )
        Task<RepositoryResult<UserLoginModel>> GetUserByEmail(string email);
    }
}
