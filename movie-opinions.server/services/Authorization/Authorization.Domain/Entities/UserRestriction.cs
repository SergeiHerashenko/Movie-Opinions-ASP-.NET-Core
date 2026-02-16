using Authorization.Domain.Common;

namespace Authorization.Domain.Entities
{
    public class UserRestriction : IBaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string Login { get; set; }

        public string? Reason { get; set; }

        public required string NameBannedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; }
    }
}