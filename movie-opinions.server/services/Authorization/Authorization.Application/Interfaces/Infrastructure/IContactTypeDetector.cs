using Authorization.Domain.Enums;
using Contracts.Models.Response;

namespace Authorization.Application.Interfaces.Infrastructure
{
    public interface IContactTypeDetector
    {
        Result<LoginType> GetLoginType(string login);
    }
}
