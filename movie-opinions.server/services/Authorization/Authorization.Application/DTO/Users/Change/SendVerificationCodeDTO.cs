using Contracts.Integration;

namespace Authorization.Application.DTO.Users.Change
{
    public class SendVerificationCodeDTO
    {
        public Guid RequestId { get; set; }

        public required string ConfirmationToken { get; set; }

        public CommunicationChannel CommunicationChannel { get; set; }
    }
}
