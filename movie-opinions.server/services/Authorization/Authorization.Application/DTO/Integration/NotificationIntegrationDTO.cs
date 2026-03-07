using Contracts.Integration;

namespace Authorization.Application.DTO.Integration
{
    public class NotificationIntegrationDTO
    {
        public Guid UserId { get; set; }

        public required string Recipient { get; set; }

        public MessageActions Action { get; set; }

        public CommunicationChannel Channel { get; set; } 
    }
}
