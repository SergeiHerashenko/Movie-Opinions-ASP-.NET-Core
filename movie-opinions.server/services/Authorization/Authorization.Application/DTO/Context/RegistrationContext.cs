using Contracts.Enum;
using Contracts.Integration;

namespace Authorization.Application.DTO.Context
{
    public class RegistrationContext
    {
        public Guid UserId { get; set; }

        public required string Login {  get; set; }

        public Role Role { get; set; }

        public CommunicationChannel Channel { get; set; }

        public MessageActions Action { get; set; }
    }
}
