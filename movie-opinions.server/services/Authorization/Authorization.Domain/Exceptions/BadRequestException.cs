using Authorization.Domain.Exceptions.BaseException;

namespace Authorization.Domain.Exceptions
{
    public class BadRequestException : BaseDomainException
    {
        public BadRequestException(string errorCode, string message) 
            : base(errorCode, message) { }
    }
}
