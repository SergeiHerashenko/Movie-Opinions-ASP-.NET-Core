using System.ComponentModel.DataAnnotations;

namespace Authorization.Domain.Request
{
    public class ChangeEmailModel
    {
        public Guid IdUser { get; set; }

        [Required]
        public string OldEmail { get; set; }

        [Required]
        public string NewEmail { get; set; }
    }
}
