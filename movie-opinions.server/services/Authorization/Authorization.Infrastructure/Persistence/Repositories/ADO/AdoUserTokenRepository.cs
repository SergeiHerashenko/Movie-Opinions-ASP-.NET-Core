using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserTokenRepository : RepositoryBase, IUserTokenRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserTokenRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserTokenRepository> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<UserToken> CreateAsync(UserToken entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    INSERT INTO 
                        User_Tokens (token_id, user_id, refresh_token, expiration_token, created_at) 
                    VALUES
                        (@Id, @UserId, @RefreshToken, @ExpirationToken, NOW())
                    RETURNING * ";

                await using (var createdTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    createdTokenCommand.Parameters.AddWithValue("@Id", entity.Id);
                    createdTokenCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    createdTokenCommand.Parameters.AddWithValue("@RefreshToken", entity.RefreshToken);
                    createdTokenCommand.Parameters.AddWithValue("@ExpirationToken", entity.RefreshTokenExpiration);

                    await using (var readerCreatedTokenCommand = await createdTokenCommand.ExecuteReaderAsync())
                    {
                        if (await readerCreatedTokenCommand.ReadAsync())
                        {
                            var newToken = MapReaderToToken(readerCreatedTokenCommand);

                            _logger.LogInformation("Токен створено для користувача {userId}!", entity.UserId);

                            return newToken;
                        }
                    }
                }

                throw new Exception("Сталась помилка при створенні токену!");
            });
        }

        public async Task<UserToken> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    DELETE FROM 
                        User_Tokens 
                    WHERE
                        token_id = @Id
                    RETURNING * ";

                await using (var deleteTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteTokenCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeleteTokenCommand = await deleteTokenCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeleteTokenCommand.ReadAsync())
                        {
                            var deleteToken = MapReaderToToken(readerDeleteTokenCommand);

                            _logger.LogInformation("Токен успішно видалений!");

                            return deleteToken;
                        }
                    }
                }

                throw new EntityNotFoundException("Сталась помилка при видаленні токену!");
            });
        }

        public async Task<UserToken> UpdateAsync(UserToken entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    UPDATE 
                        User_Tokens 
                    SET
                        user_id = @UserId,
                        refresh_token = @RefreshToken,
                        expiration_token = @ExpirationToken
                    WHERE 
                        token_id = @Id
                    RETURNING * ";

                await using (var updateTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    updateTokenCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateTokenCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateTokenCommand.Parameters.AddWithValue("@RefreshToken", entity.RefreshToken);
                    updateTokenCommand.Parameters.AddWithValue("@ExpirationToken", entity.RefreshTokenExpiration);

                    await using (var readerUpdateTokenCommand = await updateTokenCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateTokenCommand.ReadAsync())
                        {
                            var updateToken = MapReaderToToken(readerUpdateTokenCommand);

                            _logger.LogInformation("Токен успішно оновлений для користувача {userId}!", entity.UserId);

                            return updateToken;
                        }
                    }
                }

                throw new EntityNotFoundException("Сталась помилка при оновленні токену!");
            });
        }

        public async Task<UserToken> GetUserTokenAsync(string refreshToken)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        token_id, user_id, refresh_token, expiration_token, created_at 
                    FROM
                        User_Tokens
                    WHERE
                        refresh_token = @RefreshToken ";

                await using (var getTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    getTokenCommand.Parameters.AddWithValue("@RefreshToken", refreshToken);

                    await using (var readerGetTokenCommand = await getTokenCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetTokenCommand.ReadAsync())
                        {
                            var tokenEntity = MapReaderToToken(readerGetTokenCommand);

                            _logger.LogInformation("Токен знайдений!");

                            return tokenEntity;
                        }
                    }
                }

                throw new EntityNotFoundException("Сталась помилка при пошуку токену!");
            });
        }

        public async Task<IEnumerable<UserToken>> GetAllTokensUserAsync(Guid userId)
        {
            var userTokensList = new List<UserToken>();

            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        token_id, user_id, refresh_token, expiration_token, created_at 
                    FROM
                        User_Tokens
                    WHERE
                        id = @Id ";

                await using (var getTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    getTokenCommand.Parameters.AddWithValue("@Id", userId);

                    await using (var readerGetTokenCommand = await getTokenCommand.ExecuteReaderAsync())
                    {
                        while (await readerGetTokenCommand.ReadAsync())
                        {
                            var tokenEntity = MapReaderToToken(readerGetTokenCommand);

                            userTokensList.Add(tokenEntity);
                        }

                        return userTokensList;
                    }
                }

                throw new EntityNotFoundException("Сталась помилка при пошуку токену!");
            });
        }

        private UserToken MapReaderToToken(NpgsqlDataReader reader)
        {
            return new UserToken()
            {
                Id = reader.GetGuid(reader.GetOrdinal("token_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                RefreshToken = reader["refresh_token"] as string ?? string.Empty,
                RefreshTokenExpiration = Convert.ToDateTime(reader["expiration_token"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"])
            };
        }
    }
}
