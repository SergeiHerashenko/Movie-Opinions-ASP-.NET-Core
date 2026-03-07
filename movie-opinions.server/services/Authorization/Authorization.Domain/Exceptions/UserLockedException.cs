using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class UserLockedException : BaseApplicationException
    {
        public UserLockedException(string email)
            : base($"Користувача з емейлом {email} заблоковано", Auth.Locked) { }
    }
}