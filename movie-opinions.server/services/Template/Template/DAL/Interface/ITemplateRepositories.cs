using MovieOpinions.Contracts.Models.RepositoryResponse;
using Template.Model.Template;

namespace Template.DAL.Interface
{
    public interface ITemplateRepositories
    {
        Task<RepositoryResponse<TemplateEntity>> CreateTemplate(TemplateEntity templateEntity);

        Task<RepositoryResponse<TemplateEntity>> DeleteTemplate(string templateId);

        Task<RepositoryResponse<TemplateEntity>> UpdateTemplate(string templateId);

        Task<RepositoryResponse<TemplateEntity>> GetTemplate(string templateName);

        Task<List<RepositoryResponse<TemplateEntity>>> GetListTemplate(List<string> templateId);
    }
}
