using Authorization.Application.Enum;

namespace Authorization.Application.DTO.Users.Change
{
    public class ResetPasswordResponse
    {
        public ResetPasswordStep ResetPasswordStep { get; set; }

        public Guid RequestId { get; set; }

        public required string ConfirmationToken { get; set; }
    }
}
