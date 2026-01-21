namespace Authorization.Models.User
{
    public class AuthorizationUserDTO
    {
        public Guid UserId { get; set; }

        public string? Token { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
