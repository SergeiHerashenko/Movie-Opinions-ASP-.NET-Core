using Authorization.Application.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Authorization.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserLogin()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
        }

        public Guid? GetUserId()
        {
            string? userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return userId != null ? Guid.Parse(userId) : null;
        }

        public Guid? GetResetEventId()
        {
            string? eventId = _httpContextAccessor.HttpContext?.User?.FindFirst("reset_event_id")?.Value;

            return eventId != null ? Guid.Parse(eventId) : null;
        }
    }
}
