using Authorization.Application.DTO.Users;
using System.Security.Claims;

namespace Authorization.Application.Interfaces.Security.JWT
{
    public interface IUserJwtProvider
    {
        string GenerateAccessToken(UserSessionDTO user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
