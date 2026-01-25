using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Template.Model.Template;

namespace Template.DAL.Interface
{
    public interface ITemplateRepositories : IBaseRepository<TemplateEntity, RepositoryResponse<TemplateEntity>>
    {
        Task<RepositoryResponse<TemplateEntity>> GetTemplate(string templateName);

        Task<List<RepositoryResponse<TemplateEntity>>> GetListTemplate(List<string> templateId);
    }
}
