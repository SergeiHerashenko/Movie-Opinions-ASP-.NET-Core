namespace Authorization.Domain.DTO
{
    public class ProfileCreateIntegrationDTO
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }
    }
}
