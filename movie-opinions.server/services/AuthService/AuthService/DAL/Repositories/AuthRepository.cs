using AuthService.DAL.Interface;
using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.DAL.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        public Task<RepositoryResult<Guid>> CreateUser()
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResult<UserLoginModel>> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}
