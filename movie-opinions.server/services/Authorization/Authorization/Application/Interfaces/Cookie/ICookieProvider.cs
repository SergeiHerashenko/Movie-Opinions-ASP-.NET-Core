namespace Authorization.Application.Interfaces.Cookie
{
    public interface ICookieProvider
    {
        void SetAuthCookies(string accessToken, string refreshToken);

        string GetCookie(string name);

        Guid GetUserId();

        void ClearAuthCookies();
    }
}
