namespace Authorization.Application.DTO.Users.Change
{
    public class PasswordConfirmationDTO
    {
        public Guid RequestId { get; set; }

        public string? Code { get; set; }
    }
}
