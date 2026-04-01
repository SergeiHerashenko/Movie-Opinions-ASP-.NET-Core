using Authorization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Authorization.Requests
{
    public class UserLoginRequest
    {
        [EmailOrPhone]
        [Required(ErrorMessage = "Логін є обов'язковим")]
        public required string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
