using AuthService.DAL.Connect_Database;
using AuthService.DAL.Interface;
using AuthService.Models.Enums;
using AuthService.Models.Responses;
using AuthService.Models.User;
using Npgsql;

namespace AuthService.DAL.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IConnectAuthDb _connectAuthDb;

        public AuthRepository(IConnectAuthDb connectAuthDb)
        {
            _connectAuthDb = connectAuthDb;
        }

        public async Task<RepositoryResult<UserEntityDTO>> CreateUserAsync(UserEntity userEntity)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthDb.GetConnectAuthDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var transaction = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            await InsertUserTableAsync(conn, transaction, userEntity);

                            await transaction.CommitAsync();

                            return new RepositoryResult<UserEntityDTO>()
                            {
                                Data = new UserEntityDTO()
                                {
                                    UserId = userEntity.UserId,
                                    Email = userEntity.Email,
                                    IsEmailConfirmed = userEntity.IsEmailConfirmed
                                },
                                StatusCode = Models.Enums.AuthStatusCode.UserCreated,
                                Message = "Користувача створено!"
                            };
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            return new RepositoryResult<UserEntityDTO>()
                            {
                                ErrorMessage = ex.Message,
                                StatusCode = Models.Enums.AuthStatusCode.DatabaseFailure
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new RepositoryResult<UserEntityDTO>()
                    {
                        ErrorMessage = ex.Message,
                        StatusCode = Models.Enums.AuthStatusCode.InternalServerError
                    };
                }
            }
        }

        public async Task<RepositoryResult<UserEntity>> GetUserByEmailAsync(string email)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthDb.GetConnectAuthDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getUserCommand = new NpgsqlCommand(
                        "SELECT " +
                            "id_user, email_user, password_hash_user, password_salt_user, registration_date, is_email_confirmed, is_blocked, is_deleted, role " +
                        "FROM " +
                            "Users_Table " +
                        "WHERE " +
                            "email_user = @EmailUser", conn))
                    {
                        getUserCommand.Parameters.AddWithValue("@EmailUser", email);

                        await using(var readerInformationUser = await getUserCommand.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

                                return new RepositoryResult<UserEntity>()
                                {
                                    Data = userEntity,
                                    StatusCode = Models.Enums.AuthStatusCode.UserFound,
                                    Message = "Користувача знайдено!"
                                };
                            }
                        }
                    }

                    return new RepositoryResult<UserEntity>()
                    {
                        StatusCode = Models.Enums.AuthStatusCode.UserNotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResult<UserEntity>()
                    {
                        StatusCode = Models.Enums.AuthStatusCode.InternalServerError,
                        ErrorMessage = ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResult<bool>> DeleteUserAsync(Guid userId)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthDb.GetConnectAuthDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deleteUser = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Users_Table " +
                        "WHERE id_user = @ID", conn))
                    {
                        deleteUser.Parameters.AddWithValue("@ID", userId);

                        await deleteUser.ExecuteNonQueryAsync();
                    }

                    return new RepositoryResult<bool>
                    {
                        StatusCode = Models.Enums.AuthStatusCode.UserDeleted,
                        Message = "Користувача видалено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResult<bool>
                    {
                        ErrorMessage = ex.Message,
                        StatusCode = Models.Enums.AuthStatusCode.InternalServerError
                    };
                }
            }
        }

        public async Task<RepositoryResult<UserEntity>> GetUserByIdAsync(Guid userId)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthDb.GetConnectAuthDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getUserCommand = new NpgsqlCommand(
                        "SELECT " +
                            "id_user, email_user, password_hash_user, password_salt_user, registration_date, is_email_confirmed, is_blocked, is_deleted, role " +
                        "FROM " +
                            "Users_Table " +
                        "WHERE " +
                            "id_user = @IdUser", conn))
                    {
                        getUserCommand.Parameters.AddWithValue("@IdUser", userId);

                        await using (var readerInformationUser = await getUserCommand.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

                                return new RepositoryResult<UserEntity>()
                                {
                                    Data = userEntity,
                                    StatusCode = Models.Enums.AuthStatusCode.UserFound,
                                    Message = "Користувача знайдено!"
                                };
                            }
                        }
                    }

                    return new RepositoryResult<UserEntity>()
                    {
                        StatusCode = Models.Enums.AuthStatusCode.UserNotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResult<UserEntity>()
                    {
                        StatusCode = Models.Enums.AuthStatusCode.InternalServerError,
                        ErrorMessage = ex.Message
                    };
                }
            }
        }

        private async Task InsertUserTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, UserEntity entity)
        {
            await using (var insertUserTable = new NpgsqlCommand("INSERT INTO " +
                                    "Users_Table (id_user, email_user, password_hash_user, password_salt_user, registration_date, is_email_confirmed, is_blocked, is_deleted, role) " +
                                "VALUES (@Id, @Email, @PasswordHash, @PasswordSalt, NOW(), @IsEmailConfirmed, @IsBlocked, @IsDeleted, @Role);", conn, transaction))
            {
                insertUserTable.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                insertUserTable.Parameters.AddWithValue("@Email", entity.Email);
                insertUserTable.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                insertUserTable.Parameters.AddWithValue("@PasswordSalt", entity.PasswordSalt);
                insertUserTable.Parameters.AddWithValue("@IsEmailConfirmed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                insertUserTable.Parameters.AddWithValue("@IsBlocked", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                insertUserTable.Parameters.AddWithValue("@IsDeleted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                insertUserTable.Parameters.AddWithValue("@Role", (int)entity.Role);

                await insertUserTable.ExecuteNonQueryAsync();
            }
        }

        private UserEntity MapReaderToUser(NpgsqlDataReader reader)
        {
            return new UserEntity()
            {
                UserId = Guid.Parse(reader["id_user"].ToString()),
                Email = reader["email_user"].ToString(),
                PasswordHash = reader["password_hash_user"].ToString(),
                PasswordSalt = reader["password_salt_user"].ToString(),
                Role = (Role)Convert.ToInt32(reader["role"]),
                CreatedAt = Convert.ToDateTime(reader["registration_date"]),
                IsEmailConfirmed = Convert.ToBoolean(reader["is_email_confirmed"]),
                IsBlocked = Convert.ToBoolean(reader["is_blocked"]),
                IsDeleted = Convert.ToBoolean(reader["is_deleted"]),
            };
        }
    }
}
