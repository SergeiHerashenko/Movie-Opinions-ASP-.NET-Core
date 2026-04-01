using Authorization.Application.Interfaces.Identity;
using Authorization.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Authorization.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DeviceInfo GetDeviceInfo()
        {
            throw new NotImplementedException();
        }

        public string GetIpAddress()
        {
            throw new NotImplementedException();
        }

        public Guid? GetResetEventId()
        {
            string? eventId = _httpContextAccessor.HttpContext?.User?.FindFirst("reset_event_id")?.Value;

            return eventId != null ? Guid.Parse(eventId) : null;
        }

        public string GetUserAgent()
        {
            throw new NotImplementedException();
        }

        public Guid? GetUserId()
        {
            string? userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return userId != null ? Guid.Parse(userId) : null;
        }

        public string? GetUserLogin()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
        }
    }
}
