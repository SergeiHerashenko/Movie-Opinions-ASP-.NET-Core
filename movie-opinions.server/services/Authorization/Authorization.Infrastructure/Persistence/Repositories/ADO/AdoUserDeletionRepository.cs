using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletionRepository : RepositoryBase, IUserDeletionRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserDeletionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserDeletionRepository> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        public async Task<UserDeletion> CreateAsync(UserDeletion entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    INSERT INTO 
                        Users_Deleted (deletion_id, user_id, login, reason, deleted_at) 
                    VALUES 
                        (@Id, @UserId, @Login, @Reason, NOW()) 
                    RETURNING * ";

                object DbValue(object? value) => value ?? DBNull.Value;

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    deletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    deletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    deletedUserCommand.Parameters.AddWithValue("@Reason", DbValue(entity.Reason));

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserCommand.ReadAsync())
                        {
                            var newDeletedUser = MapReaderToDeleteUser(readerDeletedUserCommand);

                            _logger.LogInformation("Видалений користувач {Login}, був успішно створений. Guid {Id}. Дата створення: {Now}",
                                newDeletedUser.Login,
                                newDeletedUser.Id,
                                DateTime.UtcNow);

                            return newDeletedUser;
                        }
                    }
                }

                throw new ReturningNoDataException("Не вдалося отримати дані видаленого користувача після вставки");
            });
        }

        public async Task<UserDeletion> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

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

                            _logger.LogInformation("Інформація видаленого користувач {Login}, була успішно видалена. Guid {Id}. Дата видалення: {Now}", 
                                deletedRecordUser.Login,
                                deletedRecordUser.Id,
                                DateTime.UtcNow);

                            return deletedRecordUser;
                        }
                    }
                }

                throw new ReturningNoDataException("Сталась помилка видалення інформації з таблиці 'Видалених користувачів'.");
            });
        }

        public async Task<UserDeletion> UpdateAsync(UserDeletion entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    UPDATE 
                        Users_Deleted
                    SET 
                        user_id = @UserId,
                        login = @Login,
                        reason = @Reason
                    WHERE 
                        deletion_id = @Id
                    RETURNING * ";

                object DbValue(object? value) => value ?? DBNull.Value;

                await using (var updateDeletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateDeletedUserCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateDeletedUserCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Login", entity.Login);
                    updateDeletedUserCommand.Parameters.AddWithValue("@Reason", DbValue(entity.Reason));

                    await using (var readerUpdateDeletedUserCommand = await updateDeletedUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateDeletedUserCommand.ReadAsync())
                        {
                            var newUpdateDeletedUser = MapReaderToDeleteUser(readerUpdateDeletedUserCommand);

                            _logger.LogInformation("Інформація користувач {Login}, була успішно оновлена. Guid {Id}. Дата оновлення: {Now}",
                                newUpdateDeletedUser.Login,
                                newUpdateDeletedUser.Id,
                                DateTime.UtcNow);

                            return newUpdateDeletedUser;
                        }
                    }
                }

                throw new ReturningNoDataException("Сталась помилка оновлення інформації в таблиці 'Видалених користувачів'.");
            });
        }

        public async Task<UserDeletion?> GetUserDeletionsByIdAsync(Guid userId)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        deletion_id, user_id, login, reason, deleted_at
                    FROM 
                        Users_Deleted
                    WHERE
                        user_id = @UserId ";

                await using (var getUserDeletionByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserDeletionByIdCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerDeletedUserByIdCommand = await getUserDeletionByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserByIdCommand.ReadAsync())
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserByIdCommand);

                            _logger.LogInformation("Інформація про користувача {Login}, знайдена!", deletedRecordUser.Login);

                            return deletedRecordUser;
                        }
                    }
                }

                return null;
            });
        }

        public async Task<UserDeletion?> GetUserDeletionsByLoginAsync(string userLogin)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    SELECT 
                        deletion_id, user_id, login, reason, deleted_at
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

                            _logger.LogInformation("Інформація про користувача {Login}, знайдена!", userLogin);

                            return deletedRecordUser;
                        }
                    }
                }

                return null;
            });
        }

        private UserDeletion MapReaderToDeleteUser(NpgsqlDataReader reader)
        {
            return new UserDeletion()
            {
                Id = reader.GetGuid(reader.GetOrdinal("deletion_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Login = reader["login"] as string ?? string.Empty,
                Reason = reader["reason"] as string ?? string.Empty,
                DeletedAt = reader.GetDateTime(reader.GetOrdinal("deleted_at")),
            };
        }
    }
}