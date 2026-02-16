using Authorization.Domain.Common;
using Contracts.Enum;

namespace Authorization.Domain.Entities
{
    public class User : IBaseEntity
    {
        public Guid Id { get; set; }

        public required string Login { get; set; }

        public required string PasswordHash { get; set; }

        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsDeleted { get; set; }
    }
}