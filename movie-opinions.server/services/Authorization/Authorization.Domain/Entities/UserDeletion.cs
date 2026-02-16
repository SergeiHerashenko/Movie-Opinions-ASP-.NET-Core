using Authorization.Domain.Common;

namespace Authorization.Domain.Entities
{
    public class UserDeletion : IBaseEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string Login { get; set; }

        public string? Reason { get; set; }

        public DateTime DeletedAt { get; set; }
    }
}