using System.ComponentModel.DataAnnotations;

namespace Authorization.Application.DTO.Authentication.Request
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Логін є обов'язковим")]
        public required string Login {  get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
