namespace Authorization.Application.DTO.Users.Change
{
    public class ResetPasswordDTO
    {
        public Guid RequestId { get; set; }

        public required string ConfirmationToken  { get; set; }
    }
}
