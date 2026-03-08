using Authorization.Application.DTO.Users;

namespace Authorization.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<bool> CreateUserSessionAsync(UserSessionDTO userSessionDTO);

        Task<bool> ValidateRefreshTokenAsync(string token);

        Task<bool> DeleteSessionAsync(string token);

        Task ClearAllUserSessionsAsync(Guid userId);
    }
}
