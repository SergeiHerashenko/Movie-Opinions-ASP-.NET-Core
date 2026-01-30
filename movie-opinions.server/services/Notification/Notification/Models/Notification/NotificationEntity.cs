using Notification.Models.Enum;

namespace Notification.Models.Notification
{
    public class NotificationEntity
    {
        public Guid Id { get; set; }

        public string Destination { get; set; }

        public NotificationChannel Channel { get; set; }

        public ContentType ContentType { get; set; }

        public string NameTemplate { get; set; }

        public NotificationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
