using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Users.Response;
using Contracts.Enum;
using Contracts.Models.Response;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<Result<LoginResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO);

        Task<Result<RegistrationResponseDTO>> RegistrationAsync(UserRegistrationDTO userRegistrationDTO);

        Task<Result<Role>> LogoutAsync();

        Task<Result<LoginResponseDTO>> RefreshSessionAsync();
    }
}
