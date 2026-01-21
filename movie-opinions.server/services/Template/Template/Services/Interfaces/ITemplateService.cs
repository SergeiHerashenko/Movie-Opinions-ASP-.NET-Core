using MovieOpinions.Contracts.Models.ServiceResponse;
using Template.Model.Template;

namespace Template.Services.Interfaces
{
    public interface ITemplateService
    {
        Task<ServiceResponse<TemplateEntity>> GetTemplateText(string templateName);
    }
}
