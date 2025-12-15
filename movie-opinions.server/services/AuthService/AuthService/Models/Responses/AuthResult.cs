using AuthService.Models.Enums;

namespace AuthService.Models.Responses
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public AuthStatusCode Status { get; set; }

        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}
