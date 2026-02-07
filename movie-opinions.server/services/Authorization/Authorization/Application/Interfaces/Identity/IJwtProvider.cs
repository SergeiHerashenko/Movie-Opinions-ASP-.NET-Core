using Authorization.Domain.Models;
using System.Security.Claims;

namespace Authorization.Application.Interfaces.Identity
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(UserClaimsModel user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
