using Authorization.DAL.Connect_Database;
using Authorization.DAL.Interface;
using MovieOpinions.Contracts.Enum;
using Authorization.Models.User;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using MovieOpinions.Contracts.Models;
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

        public async Task<RepositoryResponse<UserEntityDTO>> CreateUserAsync(UserEntity userEntity)
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
                            await InsertUserTableAsync(conn, transaction, userEntity);

                            await transaction.CommitAsync();

                            return new RepositoryResponse<UserEntityDTO>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.Create.Created,
                                Message = "Користувача створено!",
                                Data = new UserEntityDTO()
                                {
                                    UserId = userEntity.UserId,
                                    Email = userEntity.Email,
                                    IsEmailConfirmed = userEntity.IsEmailConfirmed
                                },
                            };
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();

                            return new RepositoryResponse<UserEntityDTO>()
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
                    return new RepositoryResponse<UserEntityDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserEntity>> GetUserByEmailAsync(string email)
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

                                return new RepositoryResponse<UserEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача знайдено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<UserEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<UserEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<Guid>> DeleteUserAsync(Guid userId)
        {
            await using (var conn = new NpgsqlConnection(_connectAuthorizationhDb.GetConnectAuthorizationDatabase()))
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

                    return new RepositoryResponse<Guid>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCode.Delete.Ok,
                        Message = "Користувача видалено!",
                        Data = userId
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserEntity>> GetUserByIdAsync(Guid userId)
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

                                return new RepositoryResponse<UserEntity>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача знайдено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<UserEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<UserEntity>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = ex.Message
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
