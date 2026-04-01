using Authorization.Domain.Exceptions.BaseException;

namespace Authorization.Domain.Exceptions
{
    public class ForbiddenException : BaseDomainException
    {
        private const string DefaultMessage = "Запитуваний ресурс не доступний!";

        public ForbiddenException(string errorCode,  string? message = null)
            : base(errorCode, message ?? DefaultMessage) { }
    }
}
