namespace Authorization.Application.Interfaces.Http
{
    public interface ICookieProvider
    {
        void SetAuthCookies(string accessToken, string refreshToken);

        string GetCookie(string name);

        void ClearAuthCookies();
    }
}
