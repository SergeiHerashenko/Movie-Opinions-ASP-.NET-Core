using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Interfaces.Identity
{
    public interface IUserContext
    {
        string? GetUserLogin();

        Guid? GetUserId();

        Guid? GetResetEventId();

        string GetIpAddress();

        string GetUserAgent();

        DeviceInfo GetDeviceInfo();
    }
}
