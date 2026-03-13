using System.ComponentModel.DataAnnotations;

namespace Authorization.Application.DTO.Users.Change
{
    public class ChangePasswordDTO
    {
        public required string OldPassword { get; set; }

        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public required string NewPassword { get; set; }
    }
}
