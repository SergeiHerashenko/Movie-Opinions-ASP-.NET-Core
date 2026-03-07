using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class UserNotFoundException : BaseApplicationException
    {
        public UserNotFoundException(string email)
            : base($"Користувача з емейлом {email} не знайдено", General.NotFound) { }

    }
}