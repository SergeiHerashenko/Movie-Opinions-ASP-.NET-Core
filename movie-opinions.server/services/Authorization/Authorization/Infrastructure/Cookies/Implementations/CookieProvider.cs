using Authorization.Infrastructure.Cookies.Interfaces;
using System.Xml.Linq;

namespace Authorization.Infrastructure.Cookies.Implementations
{
    public class CookieProvider : ICookieProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void ClearAuthCookies()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response == null) return;

            response.Cookies.Delete("jwt");
            response.Cookies.Delete("X-Refresh-Token");
        }

        public void SetAuthCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            var response = _httpContextAccessor.HttpContext?.Response;
            if (response == null) return;

            response.Cookies.Append("jwt", accessToken,
                new CookieOptions(cookieOptions) { Expires = DateTime.UtcNow.AddMinutes(15) });

            response.Cookies.Append("X-Refresh-Token", refreshToken,
                new CookieOptions(cookieOptions) { Expires = DateTime.UtcNow.AddDays(7) });
        }
    }
}
