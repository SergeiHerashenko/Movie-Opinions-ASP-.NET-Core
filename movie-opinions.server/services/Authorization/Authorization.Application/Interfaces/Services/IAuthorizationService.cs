using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Users;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<UserResponseDTO> LoginAsync(UserLoginDTO userLoginDTO);

        Task<UserResponseDTO> RegistrationAsync(UserRegistrationDTO userRegistrationDTO);

        Task<UserResponseDTO> LogoutAsync();

        Task<UserResponseDTO> RefreshSessionAsync();
    }
}
