using MovieOpinions.Contracts.Models.RepositoryResponse;
using MovieOpinions.Contracts.Models;
using Npgsql;
using Template.DAL.Connect_Database;
using Template.DAL.Interface;
using Template.Model.Enum;
using Template.Model.Template;

namespace Template.DAL.Repositories
{
    public class TemplateRepositories : ITemplateRepositories
    {
        private readonly IConnectTemplateDb _connectTemplateDb;

        public TemplateRepositories(IConnectTemplateDb connectTemplateDb)
        {
            _connectTemplateDb = connectTemplateDb;
        }

        public Task<RepositoryResponse<TemplateEntity>> CreateTemplate(TemplateEntity templateEntity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<TemplateEntity>> DeleteTemplate(string templateId)
        {
            throw new NotImplementedException();
        }

        public Task<List<RepositoryResponse<TemplateEntity>>> GetListTemplate(List<string> templateId)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse<TemplateEntity>> GetTemplate(string templateName)
        {
            await using (var conn = new NpgsqlConnection(_connectTemplateDb.GetConnectTemplateDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getTemplate = new NpgsqlCommand(
                        "SELECT " +
                            "id_template, name_template, subject, body, channel, isHTML, createAt, type_template, category " +
                        "FROM " +
                            "TemplateText " +
                        "WHERE " +
                            "name_template = @Name_Template", conn))
                    {
                        getTemplate.Parameters.AddWithValue("@Name_Template", templateName);

                        await using (var readerInformationUser = await getTemplate.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var templateEntity = MapReaderToTemplate(readerInformationUser);

                                return new RepositoryResponse<TemplateEntity>()
                                {
                                    IsSuccess = true,
                                    Data = templateEntity,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Шаблон отриманий!"
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<TemplateEntity>()
                    {
                        IsSuccess = false,
                        Data = null,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Шаблон не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<TemplateEntity>()
                    {
                        IsSuccess = false,
                        Data = null,
                        StatusCode = StatusCode.General.InternalError,
                        Message = ex.Message
                    };
                }
            }
        }

        public Task<RepositoryResponse<TemplateEntity>> UpdateTemplate(string templateId)
        {
            throw new NotImplementedException();
        }

        private TemplateEntity MapReaderToTemplate(NpgsqlDataReader reader)
        {
            Enum.TryParse<TemplateSourceType>(reader["type_template"].ToString(), true, out var sourceType);
            Enum.TryParse<TemplateChannel>(reader["channel"].ToString(), true, out var channel);

            return new TemplateEntity()
            {
                IdTemplate = reader.GetGuid(reader.GetOrdinal("id_template")),
                Name = reader["name_template"].ToString(),
                Subject = reader["subject"]?.ToString(),
                Type = sourceType,
                Category = reader["category"].ToString(),
                Body = reader["body"].ToString(),
                Channel = channel,
                IsHtml = Convert.ToBoolean(reader["isHTML"]),
                UpdatedAt = Convert.ToDateTime(reader["createAt"])
            };
        }
    }
}
