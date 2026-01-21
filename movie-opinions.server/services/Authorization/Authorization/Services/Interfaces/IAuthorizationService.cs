using Authorization.Models.User;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Services.Interfaces
{
    public interface IAuthorizationService
    {
        Task<ServiceResponse<AuthorizationUserDTO>> LoginAsync(UserLoginModel loginModel);

        Task<ServiceResponse<AuthorizationUserDTO>> RegistrationAsync(UserRegisterModel registrationModel);
    }
}
