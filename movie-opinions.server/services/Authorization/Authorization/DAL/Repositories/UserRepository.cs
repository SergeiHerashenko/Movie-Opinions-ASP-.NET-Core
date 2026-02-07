using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;

namespace Authorization.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionProvider _dbConnection;
        private readonly ILogger<UserRepository> _logger;
        
        public UserRepository(IDbConnectionProvider dbConnection, ILogger<UserRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<RepositoryResponse<User>> CreateAsync(User entity)
        {
            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                _logger.LogInformation("Підключення до бази даних!");

                try
                {
                    await conn.OpenAsync();

                    await using (var createUser = new NpgsqlCommand(
                        "INSERT INTO " +
                            "Users (user_id, email, password_hash, password_salt, user_role, created_at, last_login_at, is_email_confirmed, is_blocked, is_deleted) " +
                        "VALUES " +
                            "(@IdUser, @EmailUser, @PasswordHash, @PasswordSalt, @UserRole, NOW(), @LastLoginAt, @IsEmailConfirm, @IsBlocked, @IsDeleted) " +
                        "RETURNING *", conn))
                    {
                        createUser.Parameters.AddWithValue("@IdUser", entity.UserId);
                        createUser.Parameters.AddWithValue("@EmailUser", entity.Email);
                        createUser.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                        createUser.Parameters.AddWithValue("@PasswordSalt", entity.PasswordSalt);
                        createUser.Parameters.AddWithValue("@UserRole", entity.Role.ToString());
                        createUser.Parameters.AddWithValue("@LastLoginAt", entity.LastLoginAt ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@IsEmailConfirm", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                        createUser.Parameters.AddWithValue("@IsBlocked", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                        createUser.Parameters.AddWithValue("@IsDeleted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;

                        await using (var readerCreatedUser = await createUser.ExecuteReaderAsync())
                        {
                            if(await readerCreatedUser.ReadAsync())
                            {
                                var newUser = MapReaderToUser(readerCreatedUser);

                                _logger.LogInformation("Користувач {Email} збережений в базу!", entity.Email);

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

                    _logger.LogError("Сталась помилка запису!");

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Сталась помилка запису!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<User>> UpdateAsync(User entity)
        {
            _logger.LogInformation("Оновлення даних користувача");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var updateUser = new NpgsqlCommand(
                        "UPDATE " +
                            "Users " +
                        "SET " +
                            "email = @UserEmail, " +
                            "password_hash = @PasswordHash, " +
                            "password_salt = @PasswordSalt " +
                            "user_role = @UserRole " +
                            "created_at = @CreatedAt " +
                            "last_login_at = @LastLoginAt " +
                            "is_email_confirmed = @IsEmailConfirmed " +
                            "is_blocked = @IsBlocked " +
                            "is_deleted = @IsDeleted " +
                        "WHERE " +
                            "user_id = @IdUser " +
                        "RETURNING * ", conn))
                    {
                        updateUser.Parameters.AddWithValue("@IdUser", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                        updateUser.Parameters.AddWithValue("@UserEmail", entity.Email);
                        updateUser.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                        updateUser.Parameters.AddWithValue("@PasswordSalt", entity.PasswordSalt);
                        updateUser.Parameters.AddWithValue("@UserRole", entity.Role.ToString());
                        updateUser.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
                        updateUser.Parameters.AddWithValue("@LastLoginAt", entity.LastLoginAt);
                        updateUser.Parameters.AddWithValue("@IsEmailConfirmed", entity.IsEmailConfirmed);
                        updateUser.Parameters.AddWithValue("@IsBlocked", entity.IsBlocked);
                        updateUser.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);

                        await using (var readerUpdateUser = await updateUser.ExecuteReaderAsync())
                        {
                            if (await readerUpdateUser.ReadAsync())
                            {
                                _logger.LogInformation("Інформація користувача оновлена");

                                return new RepositoryResponse<User>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Update.Ok,
                                    Data = MapReaderToUser(readerUpdateUser),
                                    Message = "Інформація успішно оновлена!"
                                };
                            }
                        }
                    }

                    _logger.LogWarning("Виникла помилка при оновленні інформації користувача!");

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, оновлення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<User>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public Task<RepositoryResponse<User>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse<User>> GetUserByEmailAsync(string emailUser)
        {
            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                _logger.LogInformation("Підключення до бази даних!");

                try
                {
                    await conn.OpenAsync();

                    await using (var getUserByEmail = new NpgsqlCommand(
                        "SELECT " +
                            "user_id, email, password_hash, password_salt, user_role, created_at, last_login_at, is_email_confirmed, is_blocked, is_deleted " +
                        "FROM " +
                            "Users " +
                        "WHERE " +
                            "email = @EmailUser", conn))
                    {
                        getUserByEmail.Parameters.AddWithValue("@EmailUser", emailUser);

                        await using (var readerInformationUser = await getUserByEmail.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

                                _logger.LogInformation("Користувача знайдено!");

                                return new RepositoryResponse<User>()
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

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public Task<RepositoryResponse<User>> GetUserByIdAsync(Guid idUser)
        {
            throw new NotImplementedException();
        }

        private User MapReaderToUser(NpgsqlDataReader reader)
        {
            return new User()
            {
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Email = reader["email"].ToString(),
                PasswordHash = reader["password_hash"].ToString(),
                PasswordSalt = reader["password_salt"].ToString(),
                Role = (Role)Convert.ToInt32(reader["user_role"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                LastLoginAt = Convert.ToDateTime(reader["last_login_at"]),
                IsEmailConfirmed = Convert.ToBoolean(reader["is_email_confirmed"]),
                IsBlocked = Convert.ToBoolean(reader["is_blocked"]),
                IsDeleted = Convert.ToBoolean(reader["is_deleted"]),
            };
        }
    }
}
