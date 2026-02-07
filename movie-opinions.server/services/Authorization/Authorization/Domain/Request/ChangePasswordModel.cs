using System.ComponentModel.DataAnnotations;

namespace Authorization.Domain.Request
{
    public class ChangePasswordModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
