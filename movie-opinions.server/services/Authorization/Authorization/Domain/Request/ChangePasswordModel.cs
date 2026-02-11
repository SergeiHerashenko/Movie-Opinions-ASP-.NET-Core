using System.ComponentModel.DataAnnotations;

namespace Authorization.Domain.Request
{
    public class ChangePasswordModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public string NewPassword { get; set; }
    }
}
