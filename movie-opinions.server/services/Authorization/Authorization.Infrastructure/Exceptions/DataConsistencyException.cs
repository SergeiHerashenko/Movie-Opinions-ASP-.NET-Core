using Authorization.Infrastructure.Exceptions.BaseException;

namespace Authorization.Infrastructure.Exceptions
{
    public class DataConsistencyException : BaseInfrastructureException
    {
        private const string DefaultErrorCode = "DATA_CONSISTENCY_ERROR";

        public DataConsistencyException(string message)
            : base(DefaultErrorCode, message) { }
    }
}
