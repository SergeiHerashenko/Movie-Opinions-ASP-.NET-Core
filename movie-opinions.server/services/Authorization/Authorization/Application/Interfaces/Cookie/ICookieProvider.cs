namespace Authorization.Application.Interfaces.Cookie
{
    public interface ICookieProvider
    {
        void SetAuthCookies(string accessToken, string refreshToken);

        string GetCookie(string name);

        void ClearAuthCookies();
    }
}
