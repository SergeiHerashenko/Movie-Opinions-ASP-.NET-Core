namespace Authorization.Models.User
{
    public class UserTokenModel
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = null!;

        public bool IsEmailConfirmed { get; set; }
    }
}
