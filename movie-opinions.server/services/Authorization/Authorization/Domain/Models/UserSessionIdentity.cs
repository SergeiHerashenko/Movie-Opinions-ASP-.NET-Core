using MovieOpinions.Contracts.Enum;

namespace Authorization.Domain.Models
{
    public class UserSessionIdentity
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public Role Role { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}
