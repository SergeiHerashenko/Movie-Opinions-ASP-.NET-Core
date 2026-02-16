using Contracts.Models.Status;

namespace Contracts.Models.RepositoryResponse
{
    public class RepositoryResponse<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public required string Message { get; set; }

        public required StatusCode StatusCode { get; set; }
    }
}
