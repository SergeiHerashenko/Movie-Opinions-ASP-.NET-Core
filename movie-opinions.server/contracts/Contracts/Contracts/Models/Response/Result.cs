using Contracts.Models.Status;

namespace Contracts.Models.Response
{
    public class Result
    {
        public bool IsSuccess { get; set; }

        public required string Message { get; set; }

        public required StatusCode StatusCode { get; set; }
    }

    public class Result<T> : Result
    {
        public T? Data { get; set; }
    }
}
