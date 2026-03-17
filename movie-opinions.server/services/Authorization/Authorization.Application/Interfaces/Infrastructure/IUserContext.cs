namespace Authorization.Application.Interfaces.Infrastructure
{
    public interface IUserContext
    {
        string? GetUserLogin();

        Guid? GetUserId();
    }
}
