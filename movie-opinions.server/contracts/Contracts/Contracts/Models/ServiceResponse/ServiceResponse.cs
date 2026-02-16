using Contracts.Models.Status;

namespace Contracts.Models.ServiceResponse
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }

        public required string Message { get; set; }

        public required StatusCode StatusCode { get; set; }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T? Data { get; set; }
    }
}
