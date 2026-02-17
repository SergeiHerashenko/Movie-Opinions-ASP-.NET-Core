using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Contracts.Models.RepositoryResponse;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletionRepository : IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ILogger<AdoUserDeletionRepository> _logger;

        public AdoUserDeletionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserDeletionRepository> logger)
        {
            _connectionProvider = dbConnectionProvider;
            _logger = logger;
        }

        public async Task<RepositoryResponse<UserDeletion>> CreateAsync(UserDeletion entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    INSERT INTO 
                        Users_Deleted (id, user_id, login, reason, deleted_at) 
                    VALUES 
                        (@Id, @UserId, @Login, @Reason, NOW()) 
                    RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    deletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    deletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    deletedUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await  readerDeletedUserCommand.ReadAsync())
                        {
                            var newDeletedUser = MapReaderToDeleteUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувач {Login}, був успішно збережений!", entity.Login);

                            return new RepositoryResponse<UserDeletion>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Data = newDeletedUser,
                                Message = $"Користувач {entity.Login}, був успішно збережений!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Сталась помилка запису в таблицю!");

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Сталась помилка запису в таблицю!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
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

        public async Task<RepositoryResponse<UserDeletion>> UpdateAsync(UserDeletion entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    UPDATE 
                        Users_Deleted
                    SET 
                        user_id = @UserId,
                        login = @Login,
                        reason = @Reason
                    WHERE 
                        id = @Id
                    RETURNING * ";

                await using (var updateDeletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateDeletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateDeletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);

                    await using (var readerUpdateDeletedUserCommand = await updateDeletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateDeletedUserCommand.ReadAsync())
                        {
                            var newUpdateDeletedUser = MapReaderToDeleteUser(readerUpdateDeletedUserCommand);

                            _logger.LogInformation("Інформація користувач {Login}, була успішно оновлена!", entity.Login);

                            return new RepositoryResponse<UserDeletion>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Update.Ok,
                                Data = newUpdateDeletedUser,
                                Message = $"Інформація користувача {entity.Login}, була успішно оновлена!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Сталась помилка оновлення інформації в таблицю!");

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Сталась помилка оновлення інформації в таблицю!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
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

        public async Task<RepositoryResponse<UserDeletion>> DeleteAsync(Guid id)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    DELETE FROM 
                        Users_Deleted
                    WHERE 
                        user_id = @UserId
                    RETURNING * ";

                await using (var deletedUserRecordCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserRecordCommand.Parameters.AddWithValue("@UserId", id);

                    await using (var readerDeletedUserRecordCommand = await deletedUserRecordCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserRecordCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserRecordCommand);

                            _logger.LogInformation("Інформація користувач {id}, була успішно видалена!", id);

                            return new RepositoryResponse<UserDeletion>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Delete.Ok,
                                Data = deletedRecordUser,
                                Message = $"Інформація користувач {id}, була успішно видалена"
                            };
                        }
                    }
                }

                _logger.LogWarning("Сталась помилка видалення інформації в таблицю!");

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Сталась помилка видалення інформації в таблицю!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
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

        public async Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByIdAsync(Guid userId)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    SELECT 
                        id, user_id, login, reason, deleted_at
                    FROM 
                        Users_Deleted
                    WHERE
                        user_id = @UserId ";

                await using (var getDeletedUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getDeletedUserByIdCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerGetDeletedUserByIdCommand = await getDeletedUserByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetDeletedUserByIdCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerGetDeletedUserByIdCommand);

                            _logger.LogInformation("Інформація про користувача {userId}, знайдена!", userId);

                            return new RepositoryResponse<UserDeletion>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Data = deletedRecordUser,
                                Message = $"Інформація про користувача {userId}, знайдена!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Користувача {userId} не знайдено!", userId);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {userId} не знайдено!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
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

        public async Task<RepositoryResponse<UserDeletion>> GetUserDeletionsByLoginAsync(string userLogin)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    SELECT 
                        id, user_id, login, reason, deleted_at
                    FROM 
                        Users_Deleted
                    WHERE
                        login = @Login ";

                await using (var getDeletedUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getDeletedUserByLoginCommand.Parameters.AddWithValue("@Login", userLogin);

                    await using (var readerGetDeletedUserByLoginCommand = await getDeletedUserByLoginCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetDeletedUserByLoginCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerGetDeletedUserByLoginCommand);

                            _logger.LogInformation("Інформація про користувача {userLogin}, знайдена!", userLogin);

                            return new RepositoryResponse<UserDeletion>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Data = deletedRecordUser,
                                Message = $"Інформація про користувача {userLogin}, знайдена!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Користувача {userLogin} не знайдено!", userLogin);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {userLogin} не знайдено!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserDeletion>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
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

        private UserDeletion MapReaderToDeleteUser(NpgsqlDataReader reader)
        {
            return new UserDeletion()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader["login"] as string ?? string.Empty,
                Reason = reader["reason"] as string ?? string.Empty,
                DeletedAt = reader.GetDateTime(reader.GetOrdinal("deleted_at")),
            };
        }
    }
}