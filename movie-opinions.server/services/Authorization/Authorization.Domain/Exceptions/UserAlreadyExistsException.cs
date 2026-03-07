using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class UserAlreadyExistsException : BaseApplicationException
    {
        public UserAlreadyExistsException(string email)
            : base($"Користувачз емейлом {email} вже зареєстрований", Create.Conflict) { }
    }
}