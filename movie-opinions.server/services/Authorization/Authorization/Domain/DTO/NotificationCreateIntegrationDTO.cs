namespace Authorization.Domain.DTO
{
    public class NotificationCreateIntegrationDTO
    {
        public Guid IdUser { get; set; }

        public string Recipient { get; set; }

        public string Channel { get; set; }

        public string TemplateName { get; set; }

        public Dictionary<string, string> TemplateData { get; set; }
    }
}
