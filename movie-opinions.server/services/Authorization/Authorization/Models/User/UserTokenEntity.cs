namespace Authorization.Models.User
{
    public class UserTokenEntity
    {
        public Guid IdToken { get; set; }

        public Guid IdUser { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
