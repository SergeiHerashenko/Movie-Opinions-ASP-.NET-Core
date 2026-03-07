using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Domain.Enums;

namespace Authorization.Infrastructure.Identity
{
    public class ContactTypeDetector : IContactTypeDetector
    {
        public LoginType GetLoginType(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Contact cannot be empty");

            if (login.Contains("@"))
                return LoginType.Email;

            if (long.TryParse(login.Replace("+", ""), out _))
                return LoginType.Phone;

            throw new NotSupportedException($"Contact type for '{login}' is not supported");
        }
    }
}
