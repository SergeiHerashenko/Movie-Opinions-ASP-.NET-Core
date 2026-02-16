using Authorization.Domain.Common;
using Authorization.Domain.Enums;

namespace Authorization.Domain.Entities
{
    public class UserPendingChange : IBaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string ConfirmationToken { get; set; }

        public UserChangeType UserChangeType { get; set; }

        public string? NewPasswordHash { get; set; }

        public string? NewLogin { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsConfirmed { get; set; }
    }
}