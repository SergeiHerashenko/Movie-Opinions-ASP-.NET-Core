namespace Authorization.Infrastructure.IdentityAccessor
{
    public interface IIdentityAccessor
    {
        string AccessToken { get; }

        string RefreshToken { get; }

        Guid? UserId { get; }
    }
}
