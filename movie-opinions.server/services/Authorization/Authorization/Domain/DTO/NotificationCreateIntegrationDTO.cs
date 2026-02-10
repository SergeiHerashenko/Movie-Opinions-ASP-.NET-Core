namespace Authorization.Domain.DTO
{
    public class NotificationCreateIntegrationDTO
    {
        public Guid IdUser { get; set; }

        public string Recipient { get; set; }

        public string Channel { get; set; }

        public string Action { get; set; }

        public Dictionary<string, string> Arguments { get; set; }
    }
}
