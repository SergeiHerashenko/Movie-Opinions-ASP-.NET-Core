using MovieOpinions.Contracts.Enum;

namespace Authorization.Domain.DTO
{
    public class UserResponseDTO
    {
        public Guid IdUser { get; set; }

        public string Email { get; set; }

        public Role Role { get; set; }
    }
}
