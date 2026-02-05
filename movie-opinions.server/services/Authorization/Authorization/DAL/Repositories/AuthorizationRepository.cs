using Authorization.DAL.Connect_Database;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using Authorization.Models.User;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;

namespace Authorization.DAL.Repositories
{
    public class AuthorizationRepository : IAuthorizationRepository
    {
        private readonly IConnectAuthorizationDb _connectAuthorizationhDb;

        public AuthorizationRepository(IConnectAuthorizationDb connectAuthorizationDb)
        {
            _connectAuthorizationhDb = connectAuthorizationDb;
        }

        public async Task<RepositoryResponse<User>> CreateAsync(User entity)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var transaction = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            await InsertUserTableAsync(conn, transaction, entity);

                            await transaction.CommitAsync();

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Message = "Користувача створено!",
                                Data = entity
                            };
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            return new RepositoryResponse<User>()
                            {
                                IsSuccess = false,
                                StatusCode = StatusCode.General.InternalError,
                                Message = ex.Message,
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserTokenEntity>> CreateTokenAsync(UserTokenEntity tokenEntity)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var createToken = new NpgsqlCommand(
                        "INSERT INTO " +
                            "UsersToken_Table (token_id, id_user, refresh_token, refresh_token_expiry_time, created_at) " +
                        "VALUES (@IdToken, @IdUser, @RefreshToken, @RefreshTokenExpiryTime, NOW()) " +
                        "RETURNING *", conn))
                    {
                        createToken.Parameters.AddWithValue("@IdToken", tokenEntity.IdToken);
                        createToken.Parameters.AddWithValue("@IdUser", tokenEntity.IdUser);
                        createToken.Parameters.AddWithValue("@RefreshToken", tokenEntity.RefreshToken);
                        createToken.Parameters.AddWithValue("@RefreshTokenExpiryTime", tokenEntity.RefreshTokenExpiration);

                        await using (var readerCreateToken = await createToken.ExecuteReaderAsync())
                        {
                            if (await readerCreateToken.ReadAsync())
                            {
                                return new RepositoryResponse<UserTokenEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Update.Ok,
                                    Data = MapReaderToToken(readerCreateToken),
                                    Message = "Інформація успішно оновлена!"
                                };
                            }
                        }

                        return new RepositoryResponse<UserTokenEntity>()
                        {
                            IsSuccess = false,
                            StatusCode = StatusCode.General.NotFound,
                            Message = "Токену не знайдено!"
                        };
                    }
                }
                catch(Exception ex)
                {
                    return new RepositoryResponse<UserTokenEntity>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserTokenEntity>> GetTokenAsync(string refreshToken, Guid idUser)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getToken = new NpgsqlCommand(
                        "SELECT " +
                            "token_id, id_user, refresh_token, refresh_token_expiry_time, created_at " +
                        "FROM " +
                            "UsersToken_Table " +
                        "WHERE " +
                            "refresh_token = @RefreshToken AND id_user = @IdUser", conn))
                    {
                        getToken.Parameters.AddWithValue("@RefreshToken", refreshToken);
                        getToken.Parameters.AddWithValue("@IdUser", idUser);

                        await using (var readerInformationToken = await getToken.ExecuteReaderAsync())
                        {
                            if (await readerInformationToken.ReadAsync())
                            {
                                var tokenEntity = MapReaderToToken(readerInformationToken);

                                return new RepositoryResponse<UserTokenEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача знайдено!",
                                    Data = tokenEntity
                                };
                            }
                        }

                        return new RepositoryResponse<UserTokenEntity>()
                        {
                            IsSuccess = false,
                            StatusCode = StatusCode.General.NotFound,
                            Message = "Токену не знайдено!"
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<UserTokenEntity>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<User>> UpdateAsync(User entity)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var updateUser = new NpgsqlCommand(
                        "UPDATE " +
                            "Users_Table " +
                        "SET " +
                            "email_user = @EmailUser, " +
                            "password_hash_user = @PasswordHash, " +
                            "password_salt_user = @PasswordSalt, " +
                            "registration_date = @RegistrationDate, " +
                            "is_email_confirmed = @IsEmailConfirmed, " +
                            "is_blocked = @IsBlocked, " +
                            "is_deleted = @IsDeleted, " +
                            "role = @Role " +
                        "WHERE " +
                            "id_user = @IdUser " +
                        "RETURNING *", conn))
                    {
                        updateUser.Parameters.AddWithValue("@IdUser", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                        updateUser.Parameters.AddWithValue("@EmailUser", entity.Email);
                        updateUser.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
                        updateUser.Parameters.AddWithValue("@PasswordSalt", entity.PasswordSalt);
                        updateUser.Parameters.AddWithValue("@RegistrationDate", entity.CreatedAt);
                        updateUser.Parameters.AddWithValue("@IsEmailConfirmed", entity.IsEmailConfirmed);
                        updateUser.Parameters.AddWithValue("@IsBlocked", entity.IsBlocked);
                        updateUser.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
                        updateUser.Parameters.AddWithValue("@Role", (int)entity.Role);

                        await using (var readerUpdateUser = await updateUser.ExecuteReaderAsync())
                        {
                            if(await readerUpdateUser.ReadAsync())
                            {
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

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, оновлення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<User>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<User>> DeleteAsync(Guid id)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deleteUser = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Users_Table " +
                        "WHERE " +
                            "id_user = @ID RETURNING *", conn))
                    {
                        deleteUser.Parameters.AddWithValue("@ID", id);

                        await using(var readerInformationUser = await deleteUser.ExecuteReaderAsync())
                        {
                            if(await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

                                return new RepositoryResponse<User>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача видалено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<User>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, видалення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<User>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<User>> GetUserByEmailAsync(string email)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
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

                        await using (var readerInformationUser = await getUserCommand.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

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

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<User>> GetUserByIdAsync(Guid userId)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
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

                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<User>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!" + ex.Message
                    };
                }
            }
        }

        private async Task InsertUserTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, User entity)
        {
            await using (var insertUserTable = new NpgsqlCommand(
                "INSERT INTO " +
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

        private User MapReaderToUser(NpgsqlDataReader reader)
        {
            return new User()
            {
                UserId = reader.GetGuid(reader.GetOrdinal("id_user")),
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

        private UserTokenEntity MapReaderToToken(NpgsqlDataReader reader)
        {
            return new UserTokenEntity()
            {
                IdToken = reader.GetGuid(reader.GetOrdinal("token_id")),
                IdUser = reader.GetGuid(reader.GetOrdinal("id_user")),
                RefreshToken = reader["refresh_token"].ToString(),
                RefreshTokenExpiration = Convert.ToDateTime(reader["refresh_token_expiry_time"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"])
            };
        }
    }
}