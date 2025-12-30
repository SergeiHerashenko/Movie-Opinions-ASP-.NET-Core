using Authorization.Models.Enums;

namespace Authorization.Models.Responses
{
    public class AuthorizationResult
    {
        public bool IsSuccess { get; set; }
        public AuthorizationStatusCode Status { get; set; }

        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
