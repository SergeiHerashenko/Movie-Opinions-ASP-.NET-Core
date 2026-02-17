using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Contracts.Models.RepositoryResponse;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRestrictionRepository : IUserRestrictionRepository
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ILogger<AdoUserRestrictionRepository> _logger;

        public AdoUserRestrictionRepository(IDbConnectionProvider connectionProvider, 
            ILogger<AdoUserRestrictionRepository> logger)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
        }

        public async Task<RepositoryResponse<UserRestriction>> CreateAsync(UserRestriction entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    INSERT INTO 
                        User_Restrictions (id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active) 
                    VALUES
                        (@Id, @UserId, @Login, @Reason, @NameBannedBy, NOW(), @ExpiresAt, @IsActive) 
                    RETURNING * ";

                await using (var restrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    restrictionUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    restrictionUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    restrictionUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    restrictionUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);
                    restrictionUserCommand.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                    restrictionUserCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt ?? (object)DBNull.Value);
                    restrictionUserCommand.Parameters.AddWithValue("@IsActive", true);

                    await using (var readerRestrictionUserCommand = await restrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerRestrictionUserCommand.ReadAsync())
                        {
                            var newRecord = MapReaderToBan(readerRestrictionUserCommand);

                            _logger.LogInformation("Запис про обмеження користувача {UserId} збережений в базу!", entity.UserId);

                            return new RepositoryResponse<UserRestriction>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Data = newRecord,
                                Message = $"Запис про обмеження користувача {entity.UserId} збережений в базу!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Сталась помилка запису в таблицю!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Сталась помилка запису в таблицю!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> UpdateAsync(UserRestriction entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    UPDATE 
                        User_Restrictions 
                    SET 
                        user_id = @UserId,
                        login = @Login,
                        reason = @Reason,
                        name_banned_by = @NameBannedBy,
                        expires_at = @ExpiresAt,
                        is_active = @IsActive
                    WHERE 
                        id = @Id
                    RETURNING * ";

                await using (var updateRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@IdUser", entity.UserId);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt ?? (object)DBNull.Value);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@IsActive", entity.IsActive);

                    await using (var readerUpdateRestrictionUserCommand = await updateRestrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateRestrictionUserCommand.ReadAsync())
                        {
                            var newUpdateRestrictionUser = MapReaderToBan(readerUpdateRestrictionUserCommand);

                            _logger.LogInformation("Інформація користувача {Login} успішно оновлена!", entity.Login);

                            return new RepositoryResponse<UserRestriction>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Update.Ok,
                                Data = newUpdateRestrictionUser,
                                Message = $"Інформація користувача {entity.Login} успішно оновлена!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Виникла помилка при оновленні інформації користувача!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = "Користувача не знайдено, оновлення неможливе."
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> DeleteAsync(Guid id)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                    DELETE FROM 
                        User_Restrictions
                    WHERE 
                        id = @Id 
                    RETURNING *";

                await using (var deleteRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteRestrictionUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeleteRestrictionUserCommand = await deleteRestrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeleteRestrictionUserCommand.ReadAsync())
                        {
                            var deleteRestrictionUser = MapReaderToBan(readerDeleteRestrictionUserCommand);

                            _logger.LogInformation("Запис про користувача {id} видалено!", deleteRestrictionUser.UserId);

                            return new RepositoryResponse<UserRestriction>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Delete.Ok,
                                Message = $"Запис про користувача {deleteRestrictionUser.UserId} видалено!",
                                Data = deleteRestrictionUser
                            };
                        }
                    }
                }

                _logger.LogWarning("Запис не знайдено, видалення не можливе");

                return new RepositoryResponse<UserRestriction>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = "Запис не знайдено, видалення неможливе."
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> GetActiveBanByUserIdAsync(Guid userId)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"";
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetAllBansByUserIdAsync(Guid userId)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<IEnumerable<UserRestriction>>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<IEnumerable<UserRestriction>>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> GetBanByIdAsync(Guid banId)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<UserRestriction>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<IEnumerable<UserRestriction>>() 
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL",
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<IEnumerable<UserRestriction>>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        private UserRestriction MapReaderToBan(NpgsqlDataReader reader)
        {
            return new UserRestriction()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader["login"] as string ?? string.Empty,
                Reason = reader["reason"] as string ?? string.Empty,
                NameBannedBy = reader["name_banned_by"] as string ?? string.Empty,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at")),
                IsActive = Convert.ToBoolean(reader["is_active"])
            };
        }
    }
}