using Authorization.Application.DTO.Users;

namespace Authorization.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<bool> CreateUserSessionAsync(UserSessionDTO userSessionDTO);

        string CreateTemporaryToken(Guid requestId);

        Task<UserTokenDTO?> ValidateRefreshTokenAsync();

        Task<bool> DeleteSessionAsync();

        Task ClearAllUserSessionsAsync(Guid userId);
    }
}
