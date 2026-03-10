using Authorization.Application.DTO.Users;
using System.Security.Claims;

namespace Authorization.Application.Interfaces.Security
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(UserSessionDTO user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
