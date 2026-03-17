using Authorization.Application.DTO.Integration.Responses;

namespace Authorization.Application.DTO.Users.Change
{
    public class InitiatePasswordChangeResponse
    {
        public Guid RequestId { get; set; }

        public required string ConfirmationToken { get; set; }

        public List<ContactResponseDTO>? CommunicationChannel { get; set; } 
    }
}
