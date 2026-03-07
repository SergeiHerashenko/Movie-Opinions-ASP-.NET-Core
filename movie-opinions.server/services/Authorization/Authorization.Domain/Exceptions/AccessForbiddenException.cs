using static Contracts.Models.Status.StatusCode;

namespace Authorization.Domain.Exceptions
{
    public class AccessForbiddenException : BaseApplicationException
    {
        public AccessForbiddenException()
            : base("У вас немає прав для виконання цієї операції", Auth.Forbidden) { }
    }
}