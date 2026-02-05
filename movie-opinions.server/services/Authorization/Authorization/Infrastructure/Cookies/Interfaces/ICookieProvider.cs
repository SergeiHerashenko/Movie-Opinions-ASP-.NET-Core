namespace Authorization.Infrastructure.Cookies.Interfaces
{
    public interface ICookieProvider
    {
        void SetAuthCookies(string accessToken, string refreshToken);

        void ClearAuthCookies();
    }
}
