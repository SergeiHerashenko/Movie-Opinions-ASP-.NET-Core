using Contracts.Enum;

namespace Profile.Application.DTO.Users
{
    public class CreateUserProfileDTO
    {
        public Guid UserId { get; set; }

        public required string Login { get; set; }

        public Role Role { get; set; }
    }
}
