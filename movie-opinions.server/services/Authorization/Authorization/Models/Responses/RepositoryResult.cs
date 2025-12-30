using Authorization.Models.Enums;

namespace Authorization.Models.Responses
{
    public class RepositoryResult<T>
    {
        public T? Data { get; set; }

        public string? Message { get; set; }

        public AuthorizationStatusCode StatusCode { get; set; }
    }
}
