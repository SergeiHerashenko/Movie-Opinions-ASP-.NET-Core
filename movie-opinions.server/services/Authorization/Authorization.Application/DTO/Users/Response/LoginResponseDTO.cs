using Contracts.Enum;
using System.Text.Json.Serialization;

namespace Authorization.Application.DTO.Users.Response
{
    public class LoginResponseDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Login { get; set; }

        public Role Role { get; set; }
    }
}
