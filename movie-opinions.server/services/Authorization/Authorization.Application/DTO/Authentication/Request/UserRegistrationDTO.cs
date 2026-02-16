using System.ComponentModel.DataAnnotations;

namespace Authorization.Application.DTO.Authentication.Request
{
    public class UserRegistrationDTO
    {
        [Required]
        public required string Login { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public required string Password { get; set; }

        [Required]
        [MinLength(6)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public required string ConfirmPassword { get; set; }
    }
}
