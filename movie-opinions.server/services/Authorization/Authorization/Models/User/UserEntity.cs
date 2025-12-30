using Authorization.Models.Enums;

namespace Authorization.Models.User
{
    public class UserEntity
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsDeleted { get; set; }
    }
}
