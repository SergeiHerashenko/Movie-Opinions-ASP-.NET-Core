using Contracts.Enum;

namespace Authorization.Application.DTO.Users
{
    public class UserSessionDTO
    {
        public Guid UserId { get; set; }

        public required string Login { get; set; }

        public Role Role { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}
