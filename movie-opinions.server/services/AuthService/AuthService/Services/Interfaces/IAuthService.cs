using AuthService.Models.Responses;
using AuthService.Models.User;

namespace AuthService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(UserLoginModel loginModel);

        Task<AuthResult> RegistrationAsync(UserRegisterModel registrationModel);

        string GenerateJwtToken(UserTokenModel user);
    }
}
