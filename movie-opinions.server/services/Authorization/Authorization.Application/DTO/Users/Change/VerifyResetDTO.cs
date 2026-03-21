namespace Authorization.Application.DTO.Users.Change
{
    public class VerifyResetDTO
    {
        public Guid RequestId { get; set; }

        public required string ConfirmationToken { get; set; }

        public string? Code { get; set; }
    }
}
