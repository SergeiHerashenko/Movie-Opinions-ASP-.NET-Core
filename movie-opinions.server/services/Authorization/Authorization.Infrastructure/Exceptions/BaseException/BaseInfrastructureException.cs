namespace Authorization.Infrastructure.Exceptions.BaseException
{
    public class BaseInfrastructureException : Exception
    {
        public string ErrorCode { get; }

        protected BaseInfrastructureException(string code, string? message)
            : base(string.IsNullOrWhiteSpace(message) ? "Виникла помилка серверу." : message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(code) ? "UNKNOWN_ERROR" : code;
        }
    }
}
