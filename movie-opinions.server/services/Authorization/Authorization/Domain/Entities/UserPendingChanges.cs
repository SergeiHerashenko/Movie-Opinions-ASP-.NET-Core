using Authorization.Domain.Enum;

namespace Authorization.Domain.Entities
{
    public class UserPendingChanges
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string ConfirmationToken { get; set; }

        public UserChangeType ChangeType { get; set; }

        public string? NewPasswordHash { get; set; }

        public string? NewPasswordSalt { get; set; }

        public string? NewEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsConfirmed { get; set; }
    }
}
