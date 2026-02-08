using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;

namespace Authorization.DAL.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly IDbConnectionProvider _dbConnection;
        private readonly ILogger<UserTokenRepository> _logger;

        public UserTokenRepository(IDbConnectionProvider dbConnection,
            ILogger<UserTokenRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<RepositoryResponse<UserToken>> CreateAsync(UserToken entity)
        {
            _logger.LogInformation("Підключення до бази даних!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var createRecord = new NpgsqlCommand(
                        "INSERT INTO " +
                            "User_Tokens (token_id, user_id, refresh_token, expiration_token, created_at) " +
                        "VALUES " +
                            "(@TokenId, @UserId, @RefreshToken, @ExpirationToken, NOW()) " +
                        "RETURNING *", conn))
                    {
                        createRecord.Parameters.AddWithValue("@TokenId", entity.IdToken);
                        createRecord.Parameters.AddWithValue("@UserId", entity.IdUser);
                        createRecord.Parameters.AddWithValue("@RefreshToken", entity.RefreshToken);
                        createRecord.Parameters.AddWithValue("@ExpirationToken", entity.RefreshTokenExpiration);

                        await using (var readerCreated = await createRecord.ExecuteReaderAsync())
                        {
                            if (await readerCreated.ReadAsync())
                            {
                                var newToken = MapReaderToToken(readerCreated);

                                _logger.LogInformation("Токен {IdToken} збережений в базу!", entity.IdToken);

                                return new RepositoryResponse<UserToken>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Create.Created,
                                    Data = newToken,
                                    Message = "Токен створений!"
                                };
                            }
                        }
                    }

                    _logger.LogCritical("Сталась помилка запису!");

                    return new RepositoryResponse<UserToken>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Сталась помилка запису!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserToken>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserToken>> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Початок видалення токену!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deletedToken = new NpgsqlCommand(
                        "DELETE FROM " +
                            "User_Tokens " +
                        "WHERE " +
                            "token_id = @TokenId " +
                        "RETURNING *", conn))
                    {
                        deletedToken.Parameters.AddWithValue("@TokenId", id);

                        await using (var readerInformation = await deletedToken.ExecuteReaderAsync())
                        {
                            if (await readerInformation.ReadAsync())
                            {
                                var tokenEntity = MapReaderToToken(readerInformation);

                                _logger.LogInformation("Токен видалено!");

                                return new RepositoryResponse<UserToken>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача видалено!",
                                    Data = tokenEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Токен не знайдено, видалення не можливе");

                    return new RepositoryResponse<UserToken>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Токен не знайдено, видалення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserToken>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserToken>> UpdateAsync(UserToken entity)
        {
            _logger.LogInformation("Оновлення даних токену");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var updateToken = new NpgsqlCommand(
                        "UPDATE " +
                            "User_Tokens " +
                        "SET " +
                            "user_id = @UserId, " +
                            "refresh_token = @RefreshToken, " +
                            "expiration_token = @ExpirationToken " +
                            "created_at = @CreatedAt " +
                        "WHERE " +
                            "token_id = @TokenId " +
                        "RETURNING * ", conn))
                    {
                        updateToken.Parameters.AddWithValue("@TokenId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.IdToken;
                        updateToken.Parameters.AddWithValue("@UserId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.IdUser;
                        updateToken.Parameters.AddWithValue("@RefreshToken", entity.RefreshToken);
                        updateToken.Parameters.AddWithValue("@ExpirationToken", entity.RefreshTokenExpiration);
                        updateToken.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);

                        await using (var readerUpdateToken = await updateToken.ExecuteReaderAsync())
                        {
                            if (await readerUpdateToken.ReadAsync())
                            {
                                _logger.LogInformation("Інформація токену оновлена");

                                return new RepositoryResponse<UserToken>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Update.Ok,
                                    Data = MapReaderToToken(readerUpdateToken),
                                    Message = "Інформація успішно оновлена!"
                                };
                            }
                        }
                    }

                    _logger.LogWarning("Виникла помилка при оновленні інформації токену!");

                    return new RepositoryResponse<UserToken>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Токен не знайдено, оновлення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserToken>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserToken>> GetUserTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Підключення до бази даних!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getToken = new NpgsqlCommand(
                        "SELECT " +
                            "token_id, user_id, refresh_token, expiration_token, created_at " +
                        "FROM " +
                            "User_Tokens " +
                        "WHERE " +
                            "refresh_token = @RefreshToken", conn))
                    {
                        getToken.Parameters.AddWithValue("@RefreshToken", refreshToken);

                        await using (var readerInformation = await getToken.ExecuteReaderAsync())
                        {
                            if (await readerInformation.ReadAsync())
                            {
                                var tokenEntity = MapReaderToToken(readerInformation);

                                _logger.LogInformation("Токен знайдено!");

                                return new RepositoryResponse<UserToken>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Токен знайдено!",
                                    Data = tokenEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Токену не знайдено");

                    return new RepositoryResponse<UserToken>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Токену не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserToken>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        private UserToken MapReaderToToken(NpgsqlDataReader reader)
        {
            return new UserToken()
            {
                IdToken = reader.GetGuid(reader.GetOrdinal("token_id")),
                IdUser = reader.GetGuid(reader.GetOrdinal("user_id")),
                RefreshToken = reader["refresh_token"].ToString(),
                RefreshTokenExpiration = Convert.ToDateTime(reader["expiration_token"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"])
            };
        }
    }
}