using Contracts.Integration;

namespace Authorization.Application.DTO.Integration.Responses
{
    public class ContactResponseDTO
    {
        public Guid UserId { get; set; }

        public required string ContactValue { get; set; }

        public CommunicationChannel CommunicationChannel { get; set; }
    }
}
