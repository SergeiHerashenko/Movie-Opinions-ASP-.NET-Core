using ProfileService.Models.Enums;

namespace ProfileService.Models.Responses
{
    public class ProfileResult<T>
    {
        public bool IsSuccess { get; set; }
        public ProfileStatusCode StatusCode { get; set; }

        public T? Data { get; set; }

        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
