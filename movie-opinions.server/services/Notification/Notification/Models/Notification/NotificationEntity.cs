using Notification.Models.Enum;

namespace Notification.Models.Notification
{
    public class NotificationEntity
    {
        public Guid Id { get; set; }

        public string Destination { get; set; }
        public NotificationChannel Channel { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }

        public NotificationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
