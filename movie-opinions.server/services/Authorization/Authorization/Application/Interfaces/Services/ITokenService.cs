using Authorization.Domain.DTO;
using Authorization.Domain.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<ServiceResponse<UserResponseDTO>> CreateUserSessionAsync(UserSessionIdentity user);

        Task<ServiceResponse> ClearCookies();

        Task<ServiceResponse<Guid>> ValidateAndRevokeTokenAsync();
    }
}
