using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
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
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public AdoUserRepository(IDbConnectionProvider connectionProvider,
            ILogger<AdoUserRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public async Task<User> CreateAsync(User entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return newUser;
                        }
                    }
                }

                throw new Exception("Не вдалося отримати дані після вставки");
            });
        }

        public async Task<User> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return userEntity;
                        }
                    }
                }

                throw new UserNotFoundException("Користувача не знайдено, видалення не можливе");
            });
        }

        public async Task<User> UpdateAsync(User entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return updateUser;
                        }
                    }
                }

                throw new Exception("Виникла помилка при оновленні інформації користувача!");
            });
        }

        public async Task<User?> FindUserByLoginAsync(string userLogin)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return userEntity;
                        }
                    }
                }

                return null;
            });
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return userEntity;
                        }
                    }
                }

                throw new UserNotFoundException("Користувача за id не знайдено");
            });
        }

        public async Task<User> GetUserByLoginAsync(string userLogin)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
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

                            return userEntity;
                        }
                    }
                }

                throw new UserNotFoundException("Користувача за логіном не знайдено");
            });
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