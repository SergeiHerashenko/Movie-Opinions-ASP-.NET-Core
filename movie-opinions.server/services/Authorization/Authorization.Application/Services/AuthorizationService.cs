using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Services;
using Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public Task<ServiceResponse<UserResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<UserResponseDTO>> RegistrationAsync(UserRegistrationDTO userRegistrationDTO)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<UserResponseDTO>> RefreshSessionAsync()
        {
            throw new NotImplementedException();
        }
    }
}
