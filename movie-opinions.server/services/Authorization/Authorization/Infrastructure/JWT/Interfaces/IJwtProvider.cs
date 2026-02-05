using Authorization.Models.User;
using System.Security.Claims;

namespace Authorization.Infrastructure.JWT.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(UserTokenModel user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
