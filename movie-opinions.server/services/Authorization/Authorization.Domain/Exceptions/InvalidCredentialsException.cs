using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class InvalidCredentialsException : BaseApplicationException
    {
        public InvalidCredentialsException()
            : base("Невірний логін або пароль", Auth.Unauthorized) { }
    }
}