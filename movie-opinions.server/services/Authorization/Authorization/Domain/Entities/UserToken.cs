namespace Authorization.Domain.Entities
{
    public class UserToken
    {
        public Guid IdToken { get; set; }

        public Guid IdUser { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
