namespace Authorization.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; init; }

        public bool IsFailure => !IsSuccess;

        public string? ErrorMessage { get; init; }

        public ErrorCode ErrorCode { get; init; }

        protected Result(bool isSuccess, ErrorCode errorCode, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new(true, ErrorCode.None, null);

        public static Result Failure(ErrorCode code, string message) => new(false, code, message);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T? value, bool isSuccess, ErrorCode code,  string? errorMessage)
            : base(isSuccess, code, errorMessage)
        {
            _value = value;
        }

        public bool TryGetValue(out T?  value)
        {
            value = IsSuccess ? _value : default;
            return IsSuccess;
        }

        public static Result<T> Success(T value) => new(value, true, ErrorCode.None, null);

        public static new Result<T> Failure(ErrorCode code, string message) =>
            new(default, false, code, message);
    }
}
