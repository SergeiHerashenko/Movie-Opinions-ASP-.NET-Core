using Profile.Models.Enums;

namespace Profile.Models.Responses
{
    public class RepositoryResult<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public string? Message { get; set; }

        public ProfileStatusCode StatusCode { get; set; }
    }
}
