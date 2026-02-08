using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;
using XAct.Users;

namespace Authorization.DAL.Repositories
{
    public class UserDeletionRepository : IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _dbConnection;
        private readonly ILogger<UserDeletionRepository> _logger;

        public UserDeletionRepository(IDbConnectionProvider dbConnection,
            ILogger<UserDeletionRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<RepositoryResponse<UserDeletion>> CreateAsync(UserDeletion entity)
        {
            _logger.LogInformation("Підключення до бази даних!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var createRecord = new NpgsqlCommand(
                        "INSERT INTO " +
                            "Users_Deleted (record_id, user_id, email, reason, deleted_at) " +
                        "VALUES " +
                            "(@RecordId, @UserId, @UserEmail, @Reason, NOW()) " +
                        "RETURNING *", conn))
                    {
                        createRecord.Parameters.AddWithValue("@RecordId", entity.Id);
                        createRecord.Parameters.AddWithValue("@UserId", entity.UserId);
                        createRecord.Parameters.AddWithValue("@UserEmail", entity.Email);
                        createRecord.Parameters.AddWithValue("@Reason", entity.Reason);

                        await using (var reader = await createRecord.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var newRecord = MapReaderToDelete(reader);

                                _logger.LogInformation("Користувач {Email} збережений в базу!", entity.Email);

                                return new RepositoryResponse<UserDeletion>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Create.Created,
                                    Data = newRecord,
                                    Message = "Користувач записаний в базу видалення!"
                                };
                            }
                        }
                    }

                    _logger.LogCritical("Сталась помилка запису!");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Сталась помилка запису!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserDeletion>> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Початок видалення користувача з таблиці!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deletedUser = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Users_Deleted " +
                        "WHERE " +
                            "user_id = @IdUser " +
                        "RETURNING *", conn))
                    {
                        deletedUser.Parameters.AddWithValue("@IdUser", id);

                        await using (var readerInformationUser = await deletedUser.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToDelete(readerInformationUser);

                                _logger.LogInformation("Користувача видалено з таблиці!");

                                return new RepositoryResponse<UserDeletion>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача видалено з таблиці!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Користувача не знайдено, видалення не можливе");

                    return new RepositoryResponse<UserDeletion>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, видалення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserDeletion>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserDeletion>> UpdateAsync(UserDeletion entity)
        {
            _logger.LogInformation("Оновлення даних користувача");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var updateUserInformation = new NpgsqlCommand(
                        "UPDATE " +
                            "Users_Deleted " +
                        "SET " +
                            "user_id = @IdUser, " +
                            "email = @EmailUser, " +
                            "reason = @Reason " +
                            "deleted_at = @DeletedAt " +
                        "WHERE " +
                            "record_id = @IdRecord " +
                        "RETURNING * ", conn))
                    {
                        updateUserInformation.Parameters.AddWithValue("@IdRecord", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.Id;
                        updateUserInformation.Parameters.AddWithValue("@IdUser", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                        updateUserInformation.Parameters.AddWithValue("@EmailUser", entity.Email);
                        updateUserInformation.Parameters.AddWithValue("@Reason", entity.Reason);
                        updateUserInformation.Parameters.AddWithValue("@DeletedAt", entity.DeletedAt);

                        await using (var readerUpdateUser = await updateUserInformation.ExecuteReaderAsync())
                        {
                            if (await readerUpdateUser.ReadAsync())
                            {
                                _logger.LogInformation("Інформація користувача оновлена");

                                return new RepositoryResponse<UserDeletion>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Update.Ok,
                                    Data = MapReaderToDelete(readerUpdateUser),
                                    Message = "Інформація успішно оновлена!"
                                };
                            }
                        }
                    }

                    _logger.LogWarning("Виникла помилка при оновленні інформації користувача!");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, оновлення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserDeletion>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByIdAsync(Guid idUser)
        {
            _logger.LogInformation("Підключення до бази даних!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getUserById = new NpgsqlCommand(
                        "SELECT " +
                            "record_id, user_id, email, reason, deleted_at " +
                        "FROM " +
                            "Users_Deleted " +
                        "WHERE " +
                            "user_id = @UserId", conn))
                    {
                        getUserById.Parameters.AddWithValue("@UserId", idUser);

                        await using (var readerInformationUser = await getUserById.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToDelete(readerInformationUser);

                                _logger.LogInformation("Користувача знайдено!");

                                return new RepositoryResponse<UserDeletion>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача знайдено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Користувача не знайдено");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByEmailAsync(string emailUser)
        {
            _logger.LogInformation("Підключення до бази даних!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getUserById = new NpgsqlCommand(
                        "SELECT " +
                            "record_id, user_id, email, reason, deleted_at " +
                        "FROM " +
                            "Users_Deleted " +
                        "WHERE " +
                            "email = @UserEmail", conn))
                    {
                        getUserById.Parameters.AddWithValue("@UserEmail", emailUser);

                        await using (var readerInformationUser = await getUserById.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToDelete(readerInformationUser);

                                _logger.LogInformation("Користувача знайдено!");

                                return new RepositoryResponse<UserDeletion>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача знайдено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Користувача не знайдено");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserDeletion>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        private UserDeletion MapReaderToDelete(NpgsqlDataReader reader)
        {
            return new UserDeletion()
            {
                Id = reader.GetGuid(reader.GetOrdinal("record_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Email = reader["email"].ToString(),
                Reason = reader["reason"].ToString(),
                DeletedAt = Convert.ToDateTime(reader["deleted_at"])
            };
        }
    }
}