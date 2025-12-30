using Authorization.Models.User;
using Authorization.Models.Responses;

namespace Authorization.Services.Interfaces
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResult> LoginAsync(UserLoginModel loginModel);

        Task<AuthorizationResult> RegistrationAsync(UserRegisterModel registrationModel);
    }
}
