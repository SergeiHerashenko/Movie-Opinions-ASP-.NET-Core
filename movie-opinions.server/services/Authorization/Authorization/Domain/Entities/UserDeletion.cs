namespace Authorization.Domain.Entities
{
    public class UserDeletion
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Reason { get; set; }

        public DateTime DeletedAt { get; set; }
    }
}
