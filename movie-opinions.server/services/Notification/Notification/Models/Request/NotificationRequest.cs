using Notification.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace Notification.Models.Request
{
    public class NotificationRequest
    {
        [Required]
        public Guid IdUser { get; set; }

        [Required]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public NotificationChannel Channel { get; set; }

        [Required]
        public string TemplateName { get; set; } = string.Empty;

        public Dictionary<string, string>? TemplateData { get; set; } = new();
    }
}
