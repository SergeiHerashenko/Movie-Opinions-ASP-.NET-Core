using Authorization.Domain.Enums;

namespace Authorization.Application.Interfaces.Infrastructure
{
    public interface IContactTypeDetector
    {
        LoginType GetLoginType(string login);
    }
}
