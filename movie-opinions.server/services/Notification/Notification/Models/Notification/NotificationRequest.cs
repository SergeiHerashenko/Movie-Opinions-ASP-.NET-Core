using Notification.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace Notification.Models.Notification
{
    public class NotificationRequest : IValidatableObject
    {
        public string Destination { get; private set; } = string.Empty;
        public NotificationChannel Channel { get; set; }

        public string? TemplateName { get; set; }
        public Dictionary<string, string>? TemplateData { get; set; }

        public NotificationMessage? Message { get; set; }


        public static NotificationRequest CreateDirect(string dest, NotificationChannel ch, NotificationMessage msg)
            => new() { Destination = dest, Channel = ch, Message = msg };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool hasMessage = Message != null;
            bool hasTemplate = !string.IsNullOrWhiteSpace(TemplateName);

            if (hasMessage && hasTemplate)
            {
                yield return new ValidationResult("Не можна вказувати одночасно Message та TemplateName.");
            }

            if (!hasMessage && !hasTemplate)
            {
                yield return new ValidationResult("Ви повинні вказати або готове повідомлення, або назву шаблону.");
            }

            if (hasTemplate && (TemplateData == null || TemplateData.Count == 0))
            {
                yield return new ValidationResult("Якщо вказано шаблон, необхідно передати дані (TemplateData).");
            }
        }
    }
}
