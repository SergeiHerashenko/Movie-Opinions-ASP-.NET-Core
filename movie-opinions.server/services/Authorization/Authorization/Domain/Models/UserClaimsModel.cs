namespace Authorization.Domain.Models
{
    public class UserClaimsModel
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = null!;

        public bool IsEmailConfirmed { get; set; }
    }
}
