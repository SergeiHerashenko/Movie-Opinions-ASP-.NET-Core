using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        public AdoUserRestrictionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserRestrictionRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public async Task<UserRestriction> CreateAsync(UserRestriction entity)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        INSERT INTO 
                            User_Restrictions (restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active) 
                        VALUES
                            (@Id, @UserId, @Login, @Reason, @NameBannedBy, NOW(), @ExpiresAt, @IsActive) 
                        RETURNING * ";

                await using (var restrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(restrictionUserCommand, entity);

                    await using (var readerRestrictionUserCommand = await restrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerRestrictionUserCommand.ReadAsync())
                        {
                            var newRecord = MapReaderToBan(readerRestrictionUserCommand);

                            _logger.LogInformation("Запис про обмеження користувача {Login} збережений в базу. Guid {Id}. Дата створення: {Now}",
                                newRecord.Login,
                                newRecord.Id,
                                DateTime.UtcNow);

                            return newRecord;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), entity.Id);
            });
        }

        public async Task<UserRestriction> UpdateAsync(UserRestriction entity)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
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
                            restrictions_id = @Id
                        RETURNING * ";

                await using (var updateRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateRestrictionUserCommand, entity);

                    await using (var readerUpdateRestrictionUserCommand = await updateRestrictionUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateRestrictionUserCommand.ReadAsync())
                        {
                            var newUpdateRestrictionUser = MapReaderToBan(readerUpdateRestrictionUserCommand);

                            _logger.LogInformation("Дані заблокованого користувача {Login} успішно оновлені. Guid {Id}. Дата оновлення: {Now}",
                                newUpdateRestrictionUser.Login,
                                newUpdateRestrictionUser.Id,
                                DateTime.UtcNow);

                            return newUpdateRestrictionUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), entity.Id);
            });
        }

        public async Task<UserRestriction> DeleteAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
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

                            _logger.LogInformation("Запис про користувача {Login} видалено. Guid {Id}. Дата видалення: {Now}",
                                deleteRestrictionUser.Login,
                                deleteRestrictionUser.Id,
                                DateTime.UtcNow);

                            return deleteRestrictionUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), id);
            });
        }

        public async Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
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

                            _logger.LogInformation("Заблокований користувач {Login} знайдено!", userRestriction.Login);

                            return userRestriction;
                        }
                    }
                }

                return null;
            });
        }

        public async Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var userRestrictionsList = new List<UserRestriction>();

                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
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

        public async Task<UserRestriction?> GetBanByIdAsync(Guid banId)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
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

                            _logger.LogInformation("Запис бану знайдено. Guid {Id}", restrictionEntity.Id);

                            return restrictionEntity;
                        }
                    }
                }

                return null;
            });
        }

        public async Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var bannedByAdminList = new List<UserRestriction>();

                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
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

        private static void AddParameters(NpgsqlCommand cmd, UserRestriction entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@UserId", entity.UserId);
            cmd.Parameters.AddWithValue("@Login", entity.Login);
            cmd.Parameters.AddWithValue("@Reason", DbValue(entity.Reason));
            cmd.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
            cmd.Parameters.AddWithValue("@ExpiresAt", DbValue(entity.ExpiresAt));
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        private UserRestriction MapReaderToBan(NpgsqlDataReader reader)
        {
            var restrictionsId = reader.GetGuid("restrictions_id");
            var userId = reader.GetGuid("user_id");

            var login = reader.GetString("login");

            var reason = reader.IsDBNull("reason") ? null : reader.GetString("reason");

            var nameBannedBy = reader.GetString("name_banned_by");

            var createdAt = reader.GetDateTime("created_at");

            var expiresAt = reader.IsDBNull(reader.GetOrdinal("expires_at")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("expires_at"));

            var isActive = reader.GetBoolean("is_active");

            return new UserRestriction(restrictionsId, userId, login, reason, nameBannedBy, createdAt, expiresAt, isActive);
        }
    }
}
