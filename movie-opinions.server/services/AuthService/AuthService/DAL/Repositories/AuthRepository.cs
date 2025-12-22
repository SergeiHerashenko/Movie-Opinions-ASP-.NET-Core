using AuthService.DAL.Connect_Database;
using AuthService.DAL.Interface;
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

        public async Task<RepositoryResult<UserEntityDTO>> RegistrationUserAsync(UserEntity userEntity)
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
                                    Email = userEntity.Email
                                },
                                StatusCode = Models.Enums.AuthStatusCode.Success
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
                            "id_user, email_user, passwordHash_user, passwordSalt_user " +
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
                                UserEntity userEntity = new UserEntity()
                                {
                                    UserId = Guid.Parse(readerInformationUser["id_user"].ToString()),
                                    Email = readerInformationUser["email_user"].ToString(),
                                    Password = readerInformationUser["passwordHash_user"].ToString(),
                                    SaltPassword = readerInformationUser["passwordSalt_user"].ToString()
                                };

                                return new RepositoryResult<UserEntity>()
                                {
                                    Data = userEntity,
                                    StatusCode = Models.Enums.AuthStatusCode.UserAlreadyExists
                                };
                            }
                        }
                    }

                    return new RepositoryResult<UserEntity>()
                    {
                        StatusCode = Models.Enums.AuthStatusCode.UserNotFound,
                        ErrorMessage = "Користувача не знайдено!"
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
                        StatusCode = Models.Enums.AuthStatusCode.Success
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

        private async Task InsertUserTableAsync(NpgsqlConnection conn, NpgsqlTransaction transaction, UserEntity entity)
        {
            await using (var insertUserTable = new NpgsqlCommand("INSERT INTO " +
                                    "Users_Table (id_user, email_user, passwordHash_user, passwordSalt_user, registrationDate, іsEmailConfirmed) " +
                                "VALUES (@Id, @Email, @PasswordHash, @PasswordSalt, NOW(), @IsEmailConfirmed);", conn, transaction))
            {
                insertUserTable.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                insertUserTable.Parameters.AddWithValue("@Email", entity.Email);
                insertUserTable.Parameters.AddWithValue("@PasswordHash", entity.Password);
                insertUserTable.Parameters.AddWithValue("@PasswordSalt", entity.SaltPassword);
                insertUserTable.Parameters.AddWithValue("@IsEmailConfirmed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;

                await insertUserTable.ExecuteNonQueryAsync();
            }
        }
    }
}
