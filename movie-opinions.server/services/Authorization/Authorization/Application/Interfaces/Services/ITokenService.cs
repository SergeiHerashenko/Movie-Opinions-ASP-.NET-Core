using Authorization.Domain.DTO;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<ServiceResponse<UserResponseDTO>> CreateUserSessionAsync(User user);

        Task ClearCookies();
    }
}
