using Authorization.Domain.Exceptions.BaseException;

namespace Authorization.Domain.Exceptions
{
    public class UnauthorizedException : BaseDomainException
    {
        private const string DefaultMessage = "Користувач не авторизований!";

        public UnauthorizedException(string errorCode, string?  message = null) 
            : base(errorCode, message ?? DefaultMessage) { }
    }
}
