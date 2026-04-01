namespace Authorization.Domain.Exceptions.BaseException
{
    public abstract class BaseDomainException : Exception
    {
        public string ErrorCode { get; } 

        protected BaseDomainException(string code, string? message) 
            : base(string.IsNullOrWhiteSpace(message) ? "Виникла помилка бізнес-логіки." : message) 
        {
            ErrorCode = string.IsNullOrWhiteSpace(code) ? "UNKNOWN_ERROR" : code;
        }
    }
}
