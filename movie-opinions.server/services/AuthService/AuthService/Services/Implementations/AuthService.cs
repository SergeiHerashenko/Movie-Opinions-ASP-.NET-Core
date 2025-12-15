using AuthService.Models.Responses;
using AuthService.Models.User;
using AuthService.Services.Interfaces;

namespace AuthService.Services.Implementations
{
    public class AuthService : IAuthService
    {
        public Task<AuthResult> Login(UserLoginModel loginModel)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResult> Registration(UserRegisterModel registrationModel)
        {
            throw new NotImplementedException();
        }

        public string GenerateJwtToken(UserTokenModel user)
        {
            throw new NotImplementedException();
        }
    }
}
