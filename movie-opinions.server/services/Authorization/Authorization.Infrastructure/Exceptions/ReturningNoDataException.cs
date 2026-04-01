
using Authorization.Infrastructure.Exceptions.BaseException;

namespace Authorization.Infrastructure.Exceptions
{
    public class ReturningNoDataException : BaseInfrastructureException
    {
        private const string DefaultErrorCode = "DATABASE_RECORD_NOT_FOUND";

        public ReturningNoDataException(string entityName, object identifier)
            : base(DefaultErrorCode, $"Не вдалося отримати дані для сутності '{entityName}' за ідентифікатором: {identifier}")
        {
        }
    }
}
