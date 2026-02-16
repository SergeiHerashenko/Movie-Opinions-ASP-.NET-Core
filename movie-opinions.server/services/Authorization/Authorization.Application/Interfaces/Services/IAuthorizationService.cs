using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Users;
using Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<ServiceResponse<UserResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO);

        Task<ServiceResponse<UserResponseDTO>> RegistrationAsync(UserRegistrationDTO userRegistrationDTO);

        Task<ServiceResponse<bool>> LogoutAsync();

        Task<ServiceResponse<UserResponseDTO>> RefreshSessionAsync();
    }
}
