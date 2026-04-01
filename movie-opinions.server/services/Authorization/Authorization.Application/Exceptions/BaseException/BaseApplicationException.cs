namespace Authorization.Application.Exceptions.BaseException
{
    public abstract class BaseApplicationException : Exception
    {
        public string ErrorCode { get; }

        protected BaseApplicationException(string code, string? message)
            : base(string.IsNullOrWhiteSpace(message) ? "Виникла помилка бізнес-логіки." : message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(code) ? "UNKNOWN_ERROR" : code;
        }
    }
}
