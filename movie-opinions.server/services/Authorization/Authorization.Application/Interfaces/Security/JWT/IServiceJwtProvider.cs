namespace Authorization.Application.Interfaces.Security.JWT
{
    public interface IServiceJwtProvider
    {
        string GenerateServiceToken(string serviceName, string[] permissions);
    }
}
