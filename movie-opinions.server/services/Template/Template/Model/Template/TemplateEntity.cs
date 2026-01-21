using Template.Model.Enum;

namespace Template.Model.Template
{
    public class TemplateEntity
    {
        public Guid IdTemplate { get; set; }

        public string Name { get; set; }

        public string? Subject { get; set; }

        public TemplateSourceType Type { get; set; }

        public string Category { get; set; }

        public string Body { get; set; }

        public TemplateChannel Channel { get; set; }

        public bool IsHtml { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
