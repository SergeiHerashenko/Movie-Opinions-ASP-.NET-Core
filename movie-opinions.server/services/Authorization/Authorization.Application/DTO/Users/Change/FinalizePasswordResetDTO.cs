namespace Authorization.Application.DTO.Users.Change
{
    public class FinalizePasswordResetDTO
    {
        public required string NewPassword {  get; set; }

        public required string ConfirmPassword { get; set; }
    }
}
