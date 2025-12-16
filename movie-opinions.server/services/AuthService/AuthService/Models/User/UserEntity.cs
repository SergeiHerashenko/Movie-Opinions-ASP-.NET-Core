namespace AuthService.Models.User
{
    public class UserEntity
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string SaltPassword { get; set; }
    }
}
