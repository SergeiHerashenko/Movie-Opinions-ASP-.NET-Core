using Authorization.Models.User;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces
{
    public interface IAuthorizationService
    {
        Task<ServiceResponse<AuthorizationUserDTO>> LoginAsync(UserLoginModel loginModel);

        Task<ServiceResponse<AuthorizationUserDTO>> RegistrationAsync(UserRegisterModel registrationModel);

        Task<ServiceResponse<bool>> LogoutAsync();

        Task<ServiceResponse<AuthorizationUserDTO>> RefreshTokenAsync();

        Task<ServiceResponse<AuthorizationUserDTO>> ChangePasswordAsync();
    }
}
