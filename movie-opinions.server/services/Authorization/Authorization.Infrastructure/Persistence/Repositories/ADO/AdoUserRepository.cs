using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Contracts.Enum;
using Contracts.Models.RepositoryResponse;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRepository : IUserRepository
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly ILogger<AdoUserRepository> _logger;

        public AdoUserRepository(IDbConnectionProvider connectionProvider,
            ILogger<AdoUserRepository> logger)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
        }

        public async Task<RepositoryResponse<User>> CreateAsync(User entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                        INSERT INTO 
                            Users (id, login, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted) 
                        VALUES 
                            (@Id, @Login, @PasswordHash, @Role, NOW(), @UpdatedAt, @LastLoginAt, @IsConfirmed, @IsBlocked, @IsDeleted)
                        RETURNING * ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    insertUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    insertUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    insertUserCommand.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                    insertUserCommand.Parameters.AddWithValue("@Role", entity.Role.ToString());
                    insertUserCommand.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);
                    insertUserCommand.Parameters.AddWithValue("@LastLoginAt", entity.LastLoginAt ?? (object)DBNull.Value);
                    insertUserCommand.Parameters.AddWithValue("@IsConfirmed", false);
                    insertUserCommand.Parameters.AddWithValue("@IsBlocked", false);
                    insertUserCommand.Parameters.AddWithValue("@IsDeleted", false);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerInsertUserCommand.ReadAsync())
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);

                            _logger.LogInformation("Користувач {Id} збережений в базу", newUser.Id);

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Data = newUser,
                                Message = "Користувач створений!"
                            };
                        }
                    }
                }

                _logger.LogCritical("Сталась помилка запису користувача {Id}!", entity.Id);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Сталась помилка запису!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<User>> UpdateAsync(User entity)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                        UPDATE 
                            Users 
                        SET 
                            login = @Login,
                            password_hash = @PasswordHash, 
                            user_role = @Role,
                            updated_at = @UpdatedAt,
                            last_login_at = @LastLoginAt,
                            is_confirmed = @IsConfirmed,
                            is_blocked = @IsBlocked,
                            is_deleted = @IsDeleted 
                        WHERE 
                            id = @Id
                        RETURNING * ";

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateUserCommand.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                    updateUserCommand.Parameters.AddWithValue("@Role", entity.Role.ToString());
                    updateUserCommand.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);
                    updateUserCommand.Parameters.AddWithValue("@LastLoginAt", entity.LastLoginAt ?? (object)DBNull.Value);
                    updateUserCommand.Parameters.AddWithValue("@IsConfirmed", entity.IsConfirmed);
                    updateUserCommand.Parameters.AddWithValue("@IsBlocked", entity.IsBlocked);
                    updateUserCommand.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateUserCommand.ReadAsync())
                        {
                            var updateUser = MapReaderToUser(readerUpdateUserCommand);

                            _logger.LogInformation("Дані користувача {Login} успішно оновлені!", updateUser.Login);

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Update.Ok,
                                Data = updateUser,
                                Message = $"Інформація користувача {updateUser.Login} успішно оновлена!"
                            };
                        }
                    }
                }

                _logger.LogWarning("Виникла помилка при оновленні інформації користувача {Login}!", entity.Login);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {entity.Login} не знайдено, оновлення неможливе."
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка бази даних!");

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка!"
                };
            }
        }

        public async Task<RepositoryResponse<User>> DeleteAsync(Guid id)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                        DELETE FROM 
                            Users 
                        WHERE
                            id = @Id 
                        RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувача {id} успішно видалено!", id);

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Delete.Ok,
                                Message = $"Користувача {id} успішно видалено!",
                                Data = userEntity
                            };
                        }
                    }
                }

                _logger.LogWarning("Користувача {id} не знайдено, видалення не можливе", id);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {id} не знайдено, видалення неможливе."
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка бази даних!");

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка!"
                };
            }
        }

        public async Task<RepositoryResponse<User>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                        SELECT 
                            id, login, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted 
                        FROM 
                            Users 
                        WHERE 
                            id = @Id";

                await using (var getUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByIdCommand.Parameters.AddWithValue("@Id", userId);

                    await using (var readerGetUserByIdCommand = await getUserByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetUserByIdCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readerGetUserByIdCommand);

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Message = $"Користувача {userId} знайдено!",
                                Data = userEntity
                            };
                        }
                    }
                }

                _logger.LogWarning("Користувача {userId} не знайдено", userId);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {userId} не знайдено!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        public async Task<RepositoryResponse<User>> GetUserByLoginAsync(string userLogin)
        {
            try
            {
                await using var conn = await _connectionProvider.GetOpenConnectionAsync();

                var sql = $@"
                        SELECT 
                            id, login, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted 
                        FROM 
                            Users 
                        WHERE 
                            login = @Login";

                await using (var getUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByLoginCommand.Parameters.AddWithValue("@Login", userLogin);

                    await using (var readergetUserByLoginCommand = await getUserByLoginCommand.ExecuteReaderAsync())
                    {
                        if (await readergetUserByLoginCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readergetUserByLoginCommand);

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Message = $"Користувача {userLogin} знайдено!",
                                Data = userEntity
                            };
                        }
                    }
                }

                _logger.LogWarning("Користувача {userLogin} не знайдено", userLogin);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = $"Користувача {userLogin} не знайдено!"
                };
            }
            catch (NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Помилка PostgreSQL: {Code}", ex.SqlState);

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка PostgreSQL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка баз даних!");

                return new RepositoryResponse<User>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка баз даних!"
                };
            }
        }

        private User MapReaderToUser(NpgsqlDataReader reader)
        {
            return new User()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Login = reader["login"] as string ?? string.Empty,
                PasswordHash = reader["password_hash"] as string ?? string.Empty,
                Role = Enum.TryParse<Role>(reader["user_role"]?.ToString(), out var role) ? role : Role.User,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at")),
                LastLoginAt = reader.IsDBNull(reader.GetOrdinal("last_login_at")) ? null : reader.GetDateTime(reader.GetOrdinal("last_login_at")),
                IsConfirmed = Convert.ToBoolean(reader["is_confirmed"]),
                IsBlocked = Convert.ToBoolean(reader["is_blocked"]),
                IsDeleted = Convert.ToBoolean(reader["is_deleted"]),
            };
        }
    }
}