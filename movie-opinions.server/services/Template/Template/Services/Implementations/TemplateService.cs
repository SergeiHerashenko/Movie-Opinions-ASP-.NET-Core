using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models;
using Template.DAL.Interface;
using Template.Model.Enum;
using Template.Model.Template;
using Template.Services.Interfaces;

namespace Template.Services.Implementations
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepositories _templateRepositories;
        private readonly string _baseTemplatesPath;

        public TemplateService(ITemplateRepositories templateRepositories, IWebHostEnvironment env)
        {
            _templateRepositories = templateRepositories;
            _baseTemplatesPath = Path.Combine(env.ContentRootPath, "Templates");
        }

        public async Task<ServiceResponse<TemplateEntity>> GetTemplateText(string templateName)
        {
            var getTemplate = await _templateRepositories.GetTemplate(templateName);

            if (!getTemplate.IsSuccess)
            {
                return new ServiceResponse<TemplateEntity>()
                {
                    StatusCode = getTemplate.StatusCode,
                    Message = getTemplate.Message
                };
            }

            string finalContent = "";

            try
            {
                switch (getTemplate.Data.Type)
                {
                    case TemplateSourceType.InlineText:
                        finalContent = getTemplate.Data.Body;
                        break;

                    case TemplateSourceType.File:
                        string categoryType = getTemplate.Data.Category.ToString();
                        string folderType = getTemplate.Data.Channel.ToString().Trim();
                        string fullPath = Path.Combine(_baseTemplatesPath, categoryType, folderType, getTemplate.Data.Body);

                        if (!File.Exists(fullPath))
                        {
                            return new ServiceResponse<TemplateEntity>()
                            {
                                StatusCode = StatusCode.General.NotFound,
                                Message = $"Файл шаблону не знайдено за шляхом: {fullPath}"
                            };
                        }

                        finalContent = await File.ReadAllTextAsync(fullPath);
                        break;

                    default:
                        return new ServiceResponse<TemplateEntity>()
                        {
                            StatusCode = StatusCode.General.InternalError,
                            Message = "Невідомий тип джерела шаблону."
                        };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TemplateEntity>()
                {
                    StatusCode = StatusCode.General.InternalError,
                    Message = ex.Message
                };
            }

            getTemplate.Data.Body = finalContent;

            return new ServiceResponse<TemplateEntity>()
            {
                StatusCode = StatusCode.General.Ok,
                Message = "Шаблон знайдено!",
                Data = getTemplate.Data
            };
        }
    }
}
