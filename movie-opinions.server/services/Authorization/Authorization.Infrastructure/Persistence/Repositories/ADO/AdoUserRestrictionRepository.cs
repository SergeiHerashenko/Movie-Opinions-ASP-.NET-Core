using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        private readonly IDbConnectionProvider _dbconnectionProvider;

        public AdoUserRestrictionRepository(IDbConnectionProvider connectionProvider,
            ILogger<AdoUserRestrictionRepository> logger)
                : base(logger)
        {
            _dbconnectionProvider = connectionProvider;
        }

        public async Task<UserRestriction> CreateAsync(UserRestriction entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    INSERT INTO 
                        User_Restrictions (id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active) 
                    VALUES
                        (@Id, @UserId, @Login, @Reason, @NameBannedBy, NOW(), @ExpiresAt, @IsActive) 
                    RETURNING * ";

                await using (var restrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    restrictionUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    restrictionUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    restrictionUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    restrictionUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);
                    restrictionUserCommand.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                    restrictionUserCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt ?? (object)DBNull.Value);
                    restrictionUserCommand.Parameters.AddWithValue("@IsActive", true);

                    await using (var readerRestrictionUserCommand = await restrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerRestrictionUserCommand.ReadAsync())
                        {
                            var newRecord = MapReaderToBan(readerRestrictionUserCommand);

                            _logger.LogInformation("Запис про обмеження користувача {UserId} збережений в базу!", entity.UserId);

                            return newRecord;
                        }
                    }
                }

                throw new Exception("Не вдалося отримати дані після вставки");
            });
        }

        public async Task<UserRestriction> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    DELETE FROM 
                        User_Restrictions
                    WHERE 
                        id = @Id 
                    RETURNING *";

                await using (var deleteRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteRestrictionUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeleteRestrictionUserCommand = await deleteRestrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeleteRestrictionUserCommand.ReadAsync())
                        {
                            var deleteRestrictionUser = MapReaderToBan(readerDeleteRestrictionUserCommand);

                            _logger.LogInformation("Запис про користувача {UserId} видалено!", deleteRestrictionUser.UserId);

                            return deleteRestrictionUser;
                        }
                    }
                }

                throw new EntityNotFoundException("Запис не знайдено, видалення не можливе");
            });
        }

        public async Task<UserRestriction> UpdateAsync(UserRestriction entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    UPDATE 
                        User_Restrictions 
                    SET 
                        user_id = @UserId,
                        login = @Login,
                        reason = @Reason,
                        name_banned_by = @NameBannedBy,
                        expires_at = @ExpiresAt,
                        is_active = @IsActive
                    WHERE 
                        id = @Id
                    RETURNING * ";

                await using (var updateRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt ?? (object)DBNull.Value);
                    updateRestrictionUserCommand.Parameters.AddWithValue("@IsActive", entity.IsActive);

                    await using (var readerUpdateRestrictionUserCommand = await updateRestrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateRestrictionUserCommand.ReadAsync())
                        {
                            var newUpdateRestrictionUser = MapReaderToBan(readerUpdateRestrictionUserCommand);

                            _logger.LogInformation("Інформація користувача {Login} успішно оновлена!", entity.Login);

                            return newUpdateRestrictionUser;
                        }
                    }
                }

                throw new EntityNotFoundException("Виникла помилка при оновленні інформації користувача!");
            });
        }

        public async Task<UserRestriction> GetActiveBanByUserIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                    FROM 
                        User_Restrictions
                    WHERE 
                        user_id = @UserId
                    AND 
                        is_active = true";

                await using (var getActiveUserRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getActiveUserRestrictionCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerActiveUserRestriction = await getActiveUserRestrictionCommand.ExecuteReaderAsync())
                    {
                        if (await readerActiveUserRestriction.ReadAsync())
                        {
                            var userRestriction = MapReaderToBan(readerActiveUserRestriction);

                            _logger.LogInformation("Заблокований користувач {userId} знайдено!", userId);

                            return userRestriction;
                        }
                    }
                }

                throw new EntityNotFoundException($"Активних блокувань для користувача не знайдено!");
            });
        }

        public async Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                var userRestrictionsList = new List<UserRestriction>();

                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                    FROM 
                        User_Restrictions
                    WHERE 
                        user_id = @UserId ";

                await using (var getAllUserRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getAllUserRestrictionCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerAllUserRestriction = await getAllUserRestrictionCommand.ExecuteReaderAsync())
                    {
                        while (await readerAllUserRestriction.ReadAsync())
                        {
                            var userRestriction = MapReaderToBan(readerAllUserRestriction);

                            userRestrictionsList.Add(userRestriction);
                        }

                        return userRestrictionsList;
                    }
                }
            });
        }

        public async Task<UserRestriction> GetBanByIdAsync(Guid banId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                    FROM 
                        User_Restrictions
                    WHERE 
                        id = @Id ";

                await using (var getRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getRestrictionCommand.Parameters.AddWithValue("@Id", banId);

                    await using (var readerGetRestrictionCommand = await getRestrictionCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetRestrictionCommand.ReadAsync())
                        {
                            var restrictionEntity = MapReaderToBan(readerGetRestrictionCommand);

                            _logger.LogInformation("Запис бану знайдено!");

                            return restrictionEntity;
                        }
                    }
                }

                throw new EntityNotFoundException($"Запис бану з ID {banId} не знайдено");
            });
        }

        public async Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            return await ExecuteAsync(async () =>
            {
                var bannedByAdminList = new List<UserRestriction>();

                await using var conn = await _dbconnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                    FROM 
                        User_Restrictions
                    WHERE 
                        name_banned_by = @NameBannedBy";

                await using (var bannedByAdminCommand = new NpgsqlCommand(sql, conn))
                {
                    bannedByAdminCommand.Parameters.AddWithValue("@NameBannedBy", adminNickname);

                    await using (var readerBannedByAdminCommand = await bannedByAdminCommand.ExecuteReaderAsync())
                    {

                        while (await readerBannedByAdminCommand.ReadAsync())
                        {
                            var banRecord = MapReaderToBan(readerBannedByAdminCommand);

                            bannedByAdminList.Add(banRecord);
                        }

                        return bannedByAdminList;
                    }
                }
            });
        }

        private UserRestriction MapReaderToBan(NpgsqlDataReader reader)
        {
            return new UserRestriction()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader.GetFieldValue<string>(reader.GetOrdinal("login")),
                Reason = reader.IsDBNull(reader.GetOrdinal("reason")) ? null : reader.GetString(reader.GetOrdinal("reason")),
                NameBannedBy = reader.GetString(reader.GetOrdinal("name_banned_by")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ExpiresAt = reader.IsDBNull(reader.GetOrdinal("expires_at")) ? null : reader.GetDateTime(reader.GetOrdinal("expires_at")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            };
        }
    }
}