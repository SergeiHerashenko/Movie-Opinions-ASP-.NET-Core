using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;
using Verification.DAL.Connect_Database;
using Verification.DAL.Interface;
using Verification.Models;

namespace Verification.DAL.Repositories
{
    public class VerificationRepositories : IVerificationRepositories
    {
        private readonly IConnectVerificationDb _connectVerificationDb;

        public VerificationRepositories(IConnectVerificationDb connectVerificationDb)
        {
            _connectVerificationDb = connectVerificationDb;
        }

        public async Task<RepositoryResponse<VerificationEntity>> CreateAsync(VerificationEntity entity)
        {
            await using (var conn = new NpgsqlConnection(_connectVerificationDb.GetConnectVerificatioDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var transaction = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            await InsertVerificationTableAsync(conn, transaction, entity);

                            await transaction.CommitAsync();

                            return new RepositoryResponse<VerificationEntity>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Message = "Запис створено!",
                                Data = entity
                            };
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            return new RepositoryResponse<VerificationEntity>()
                            {
                                IsSuccess = false,
                                StatusCode = StatusCode.General.InternalError,
                                Message = "Не вдалось створити запис!" + ex.Message,
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<VerificationEntity>()
                    {
                        IsSuccess = false,
                        Message = "Критична помилка!" + ex.Message,
                        StatusCode = StatusCode.General.InternalError
                    };
                }
            }
        }

        public Task<RepositoryResponse<VerificationEntity>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<Guid>> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<VerificationEntity>> UpdateAsync(VerificationEntity entity)
        {
            throw new NotImplementedException();
        }

        private async Task InsertVerificationTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, VerificationEntity entity)
        {
            await using (var insertUserTable = new NpgsqlCommand(
                "INSERT INTO " +
                    "Verification_Table (id, id_user, code, type, created_at, expiry_date) " +
                "VALUES " +
                    "(@Id, @IdUser, @Code, @Type, NOW(), NOW());", conn, transaction))
            {
                insertUserTable.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.Id;
                insertUserTable.Parameters.AddWithValue("@IdUser", entity.UserId);
                insertUserTable.Parameters.AddWithValue("@Code", entity.Code);
                insertUserTable.Parameters.AddWithValue("@Type", entity.Type);

                await insertUserTable.ExecuteNonQueryAsync();
            }
        }
    }
}
