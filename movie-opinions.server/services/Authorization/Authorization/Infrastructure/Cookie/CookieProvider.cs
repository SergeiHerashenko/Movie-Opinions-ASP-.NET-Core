using Authorization.Application.Interfaces.Cookie;

namespace Authorization.Infrastructure.Cookie
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
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            context.Response.Cookies.Delete("jwt", cookieOptions);
            context.Response.Cookies.Delete("X-Refresh-Token", cookieOptions);
        }

        public string GetCookie(string name)
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[name];
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
