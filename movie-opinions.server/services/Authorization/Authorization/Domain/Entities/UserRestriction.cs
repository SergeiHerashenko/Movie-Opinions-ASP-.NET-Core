namespace Authorization.Domain.Entities
{
    public class UserRestriction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Reason { get; set; }

        public string NameBannedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; }
    }
}
