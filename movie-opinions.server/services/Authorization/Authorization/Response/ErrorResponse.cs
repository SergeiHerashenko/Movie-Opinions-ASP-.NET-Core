namespace Authorization.Response
{
    public class ErrorResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = null!;

        public int StatusCode { get; set; }

        public string ErrorCode { get; set; } = null!;
    }
}
