namespace Authorization.Application.DTO.Integration
{
    public class ProfileIntegrationDTO
    {
        public Guid UserId { get; set; }

        public required string Login { get; set; }
    }
}
