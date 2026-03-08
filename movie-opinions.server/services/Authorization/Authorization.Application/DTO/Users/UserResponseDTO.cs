using Authorization.Application.Enum;
using Contracts.Enum;
using System.Text.Json.Serialization;

namespace Authorization.Application.DTO.Users
{
    public class UserResponseDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required string? Login { get; set; }

        public Role Role { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RegistrationStep? NextStep { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }
    }
}
