using Authorization.Domain.Exceptions.BaseException;

namespace Authorization.Domain.Exceptions
{
    public class NotFoundException : BaseDomainException
    {
        private const string DefaultMessage = "Запитуваний ресурс не знайдено!";

        public NotFoundException(string errorCode, string? message = null) 
            : base(errorCode, message ?? DefaultMessage) { }
    }
}
