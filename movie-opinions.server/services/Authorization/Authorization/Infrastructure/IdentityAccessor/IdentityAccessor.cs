using Authorization.Infrastructure.JWT.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Authorization.Infrastructure.IdentityAccessor
{
    public class IdentityAccessor : IIdentityAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtProvider _jwtProvider;

        public IdentityAccessor(IHttpContextAccessor httpContextAccessor, IJwtProvider jwtProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtProvider = jwtProvider;
        }

        public string AccessToken => _httpContextAccessor.HttpContext.Request.Cookies["jwt"];

        public string RefreshToken => _httpContextAccessor.HttpContext.Request.Cookies["X-Refresh-Token"];

        public Guid? UserId
        {
            get
            {
                var token = AccessToken;
                if (string.IsNullOrEmpty(token)) return null;

                var principal = _jwtProvider.GetPrincipalFromExpiredToken(token);
                var userIdStr = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                return Guid.TryParse(userIdStr, out var userId) ? userId : null;
            }
        }
    }
}
