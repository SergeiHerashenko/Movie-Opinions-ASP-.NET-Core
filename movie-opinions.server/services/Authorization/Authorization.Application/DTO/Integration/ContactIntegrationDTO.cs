using Contracts.Integration;

namespace Authorization.Application.DTO.Integration
{
    public class ContactIntegrationDTO
    {
        public Guid UserId { get; set; }

        public required string ContactValue { get; set; }

        public CommunicationChannel CommunicationChannel { get; set; }
    }
}
