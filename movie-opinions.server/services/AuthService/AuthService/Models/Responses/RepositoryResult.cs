using AuthService.Models.Enums;

namespace AuthService.Models.Responses
{
    public class RepositoryResult<T>
    {
        public T? Data { get; set; }

        public string? ErrorMessage { get; set; }

        public AuthStatusCode StatusCode { get; set; }
    }
}
