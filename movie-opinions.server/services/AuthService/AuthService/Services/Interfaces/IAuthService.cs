using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Login(UserLoginModel loginModel);

        Task<AuthResult> Registration(UserRegisterModel registrationModel);

        string GenerateJwtToken(UserTokenModel user);
    }
}
