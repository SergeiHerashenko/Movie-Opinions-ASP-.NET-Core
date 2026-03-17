using Contracts.Models.Status;

namespace Authorization.Application.DTO.Validator
{
    public class ValidationResult<T>
    {
        public bool IsSuccess { get; set; }

        public required string Message { get; set; }

        public required StatusCode StatusCode { get; set; }

        public T? Data { get; set; }
    }
}
