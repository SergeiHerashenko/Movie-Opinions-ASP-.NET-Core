using Contracts.Models.Status;

namespace Contracts.Models.ServiceResult
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }

        public required StatusCode StatusCode { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }
    }
}
