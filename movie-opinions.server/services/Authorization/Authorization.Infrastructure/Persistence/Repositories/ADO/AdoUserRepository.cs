using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Contracts.Enum;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRepository : RepositoryBase, IUserRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserRepository(IDbConnectionProvider dbconnectionProvider,
            ILogger<AdoUserRepository> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbconnectionProvider;
        }

        public async Task<User> CreateAsync(User entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();
            
                var sql = @"
                    INSERT INTO 
                        Users (user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted) 
                    VALUES 
                        (@Id, @Login, @LoginType, @PasswordHash, @Role, NOW(), @UpdatedAt, @LastLoginAt, @IsConfirmed, @IsBlocked, @IsDeleted)
                    RETURNING * ";
            
                object DbValue(object? value) => value ?? DBNull.Value;
            
                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    insertUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    insertUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    insertUserCommand.Parameters.AddWithValue("@LoginType", entity.LoginType.ToString());
                    insertUserCommand.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                    insertUserCommand.Parameters.AddWithValue("@Role", entity.Role.ToString());
                    insertUserCommand.Parameters.AddWithValue("@UpdatedAt", DbValue(entity.UpdatedAt));
                    insertUserCommand.Parameters.AddWithValue("@LastLoginAt", DbValue(entity.LastLoginAt));
                    insertUserCommand.Parameters.AddWithValue("@IsConfirmed", false);
                    insertUserCommand.Parameters.AddWithValue("@IsBlocked", false);
                    insertUserCommand.Parameters.AddWithValue("@IsDeleted", false);
            
                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerInsertUserCommand.ReadAsync())
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);
            
                            _logger.LogInformation("Користувач {Login} збережений в базу. Guid {Id}. Дата створення: {Now}",
                                newUser.Login, 
                                newUser.Id, 
                                DateTime.UtcNow);
            
                            return newUser;
                        }
                    }
                }
            
                throw new ReturningNoDataException("Не вдалося отримати дані при створенні користувача");
            });
        }

        public async Task<User> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        DELETE FROM 
                            Users 
                        WHERE
                            user_id = @Id 
                        RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувача {Login} успішно видалено. Guid {Id}. Дата видалення: {Now}", 
                                userEntity.Login,
                                userEntity.Id,
                                DateTime.UtcNow);

                            return userEntity;
                        }
                    }
                }

                throw new ReturningNoDataException("Користувача не знайдено, видалення не можливе");
            });
        }

        public async Task<User> UpdateAsync(User entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        UPDATE 
                            Users 
                        SET 
                            login = @Login,
                            login_type = @LoginType,
                            password_hash = @PasswordHash, 
                            user_role = @Role,
                            updated_at = @UpdatedAt,
                            last_login_at = @LastLoginAt,
                            is_confirmed = @IsConfirmed,
                            is_blocked = @IsBlocked,
                            is_deleted = @IsDeleted 
                        WHERE 
                            user_id = @Id
                        RETURNING * ";

                object DbValue(object? value) => value ?? DBNull.Value;

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateUserCommand.Parameters.AddWithValue("@LoginType", entity.LoginType.ToString());
                    updateUserCommand.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                    updateUserCommand.Parameters.AddWithValue("@Role", entity.Role.ToString());
                    updateUserCommand.Parameters.AddWithValue("@UpdatedAt", DbValue(entity.UpdatedAt));
                    updateUserCommand.Parameters.AddWithValue("@LastLoginAt", DbValue(entity.LastLoginAt));
                    updateUserCommand.Parameters.AddWithValue("@IsConfirmed", entity.IsConfirmed);
                    updateUserCommand.Parameters.AddWithValue("@IsBlocked", entity.IsBlocked);
                    updateUserCommand.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateUserCommand.ReadAsync())
                        {
                            var updateUser = MapReaderToUser(readerUpdateUserCommand);

                            _logger.LogInformation("Дані користувача {Login} успішно оновлені. Guid {Id}. Дата оновлення: {Now}", 
                                updateUser.Login,
                                updateUser.Id,
                                DateTime.UtcNow);

                            return updateUser;
                        }
                    }
                }

                throw new ReturningNoDataException("Не вдалося отримати дані при оновленні користувача");
            });
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        SELECT 
                            user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted 
                        FROM 
                            Users 
                        WHERE 
                            user_id = @Id";

                await using (var getUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByIdCommand.Parameters.AddWithValue("@Id", userId);

                    await using (var readerGetUserByIdCommand = await getUserByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetUserByIdCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readerGetUserByIdCommand);

                            return userEntity;
                        }
                    }
                }

                return null;
            });
        }

        public async Task<User?> GetUserByLoginAsync(string userLogin)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        SELECT 
                            user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, is_confirmed, is_blocked, is_deleted 
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

                            return userEntity;
                        }
                    }
                }

                return null;
            });
        }

        private User MapReaderToUser(NpgsqlDataReader reader)
        {
            return new User()
            {
                Id = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader["login"] as string ?? string.Empty,
                LoginType = Enum.TryParse<LoginType>(reader["login_type"]?.ToString(), out var login) ? login : LoginType.Email,
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