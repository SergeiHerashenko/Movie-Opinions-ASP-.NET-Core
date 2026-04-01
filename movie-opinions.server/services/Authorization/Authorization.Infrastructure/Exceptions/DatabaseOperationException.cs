using Authorization.Infrastructure.Exceptions.BaseException;

namespace Authorization.Infrastructure.Exceptions
{
    public class DatabaseOperationException : BaseInfrastructureException
    {
        public DatabaseOperationException(string code, string message)
            : base(code, message) { }
    }
}
