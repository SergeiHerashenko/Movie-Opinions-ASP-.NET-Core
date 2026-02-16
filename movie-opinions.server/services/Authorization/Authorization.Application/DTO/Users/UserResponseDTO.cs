using Contracts.Enum;

namespace Authorization.Application.DTO.Users
{
    public class UserResponseDTO
    {
        public required string Login { get; set; }

        public Role Role { get; set; }
    }
}
