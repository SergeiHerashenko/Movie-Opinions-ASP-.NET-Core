using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletionRepository : RepositoryBase, IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _dbonnectionProvider;

        public AdoUserDeletionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserDeletionRepository> logger)
                : base(logger)
        {
            _dbonnectionProvider = dbConnectionProvider;
        }

        public async Task<UserDeletion> CreateAsync(UserDeletion entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbonnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    INSERT INTO 
                        Users_Deleted (id, user_id, login, reason, deleted_at) 
                    VALUES 
                        (@Id, @UserId, @Login, @Reason, NOW()) 
                    RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    deletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    deletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    deletedUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserCommand.ReadAsync())
                        {
                            var newDeletedUser = MapReaderToDeleteUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувач {Login}, був успішно збережений!", entity.Login);

                            return newDeletedUser;
                        }
                    }
                }

                throw new Exception("Не вдалося отримати дані після вставки");
            });
        }

        public async Task<UserDeletion> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbonnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    DELETE FROM 
                        Users_Deleted
                    WHERE 
                        user_id = @UserId
                    RETURNING * ";

                await using (var deletedUserRecordCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserRecordCommand.Parameters.AddWithValue("@UserId", id);

                    await using (var readerDeletedUserRecordCommand = await deletedUserRecordCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserRecordCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserRecordCommand);

                            _logger.LogInformation("Інформація користувач {id}, була успішно видалена!", id);

                            return deletedRecordUser;
                        }
                    }
                }

                throw new Exception("Сталась помилка видалення інформації з таблиці!");
            });
        }

        public async Task<UserDeletion> UpdateAsync(UserDeletion entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbonnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    UPDATE 
                        Users_Deleted
                    SET 
                        user_id = @UserId,
                        login = @Login,
                        reason = @Reason
                    WHERE 
                        id = @Id
                    RETURNING * ";

                await using (var updateDeletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateDeletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateDeletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Reason", entity.Reason ?? (object)DBNull.Value);

                    await using (var readerUpdateDeletedUserCommand = await updateDeletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateDeletedUserCommand.ReadAsync())
                        {
                            var newUpdateDeletedUser = MapReaderToDeleteUser(readerUpdateDeletedUserCommand);

                            _logger.LogInformation("Інформація користувач {Login}, була успішно оновлена!", entity.Login);

                            return newUpdateDeletedUser;
                        }
                    }
                }

                throw new Exception("Сталась помилка оновлення інформації в таблиці!");
            });
        }

        public async Task<UserDeletion> GetUserDeletionsByIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbonnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, deleted_at
                    FROM 
                        Users_Deleted
                    WHERE
                        user_id = @UserId ";

                await using (var getDeletedUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getDeletedUserByIdCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerGetDeletedUserByIdCommand = await getDeletedUserByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetDeletedUserByIdCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerGetDeletedUserByIdCommand);

                            _logger.LogInformation("Інформація про користувача {userId}, знайдена!", userId);

                            return deletedRecordUser;
                        }
                    }
                }

                throw new Exception("Сталась помилка при видаленні інформації в таблиці!");
            });
        }

        public async Task<UserDeletion> GetUserDeletionsByLoginAsync(string userLogin)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbonnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        id, user_id, login, reason, deleted_at
                    FROM 
                        Users_Deleted
                    WHERE
                        login = @Login ";

                await using (var getDeletedUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getDeletedUserByLoginCommand.Parameters.AddWithValue("@Login", userLogin);

                    await using (var readerGetDeletedUserByLoginCommand = await getDeletedUserByLoginCommand.ExecuteReaderAsync())
                    {
                        if (await readerGetDeletedUserByLoginCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerGetDeletedUserByLoginCommand);

                            _logger.LogInformation("Інформація про користувача {userLogin}, знайдена!", userLogin);

                            return deletedRecordUser;
                        }
                    }
                }

                throw new Exception("Сталась помилка при видаленні інформації в таблиці!");
            });
        }

        private UserDeletion MapReaderToDeleteUser(NpgsqlDataReader reader)
        {
            return new UserDeletion()
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader["login"] as string ?? string.Empty,
                Reason = reader["reason"] as string ?? string.Empty,
                DeletedAt = reader.GetDateTime(reader.GetOrdinal("deleted_at")),
            };
        }
    }
}