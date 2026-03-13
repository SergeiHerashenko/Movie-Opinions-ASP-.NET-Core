using Authorization.Application.Enum;
using Contracts.Enum;
using System.Text.Json.Serialization;

namespace Authorization.Application.DTO.Users.Response
{
    public class RegistrationResponseDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Login { get; set; }

        public RegistrationStep RegistrationStep { get; set; }

        public Role Role { get; set; }
    }
}
