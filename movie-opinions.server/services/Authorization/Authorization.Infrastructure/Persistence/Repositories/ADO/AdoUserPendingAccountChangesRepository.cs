using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserPendingAccountChangesRepository : RepositoryBase, IUserPendingAccountChangesRepository
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;

        public AdoUserPendingAccountChangesRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserPendingAccountChangesRepository> logger)
                : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }


        public async Task<UserPendingChange> CreateAsync(UserPendingChange entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                    INSERT INTO 
                        User_Changes (change_id, user_id, confirmation_token, change_type, new_password_hash, new_login, created_at, expires_at, is_confirmed) 
                    VALUES 
                        (@Id, @UserId, @ConfirmToken, @ChangeType, @NewPasswordHash, @NewLogin, NOW(), @ExpiresAt, @IsConfirmed)
                    RETURNING * ";

                object DbValue(object? value) => value ?? DBNull.Value;

                await using (var insertChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    insertChangeCommand.Parameters.AddWithValue("@Id", entity.Id);
                    insertChangeCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    insertChangeCommand.Parameters.AddWithValue("@ConfirmToken", entity.ConfirmationToken.ToString());
                    insertChangeCommand.Parameters.AddWithValue("@ChangeType", entity.UserChangeType.ToString());
                    insertChangeCommand.Parameters.AddWithValue("@NewPasswordHash", DbValue(entity.NewPasswordHash));
                    insertChangeCommand.Parameters.AddWithValue("@NewLogin", DbValue(entity.NewLogin));
                    insertChangeCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
                    insertChangeCommand.Parameters.AddWithValue("@IsConfirmed", false);

                    await using (var readerInsertChangeCommand = await insertChangeCommand.ExecuteReaderAsync())
                    {
                        if (await readerInsertChangeCommand.ReadAsync())
                        {
                            var changeEntity = MapReaderToChange(readerInsertChangeCommand);

                            _logger.LogInformation("Запис {Id} збережений в базу. Дата створення: {Now}",
                                changeEntity.Id,
                                DateTime.UtcNow);

                            return changeEntity;
                        }
                    }
                }

                throw new ReturningNoDataException("Не вдалося отримати дані при створенні тимчасових даних");
            });
        }

        public async Task<UserPendingChange> DeleteAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        DELETE FROM 
                            User_Changes 
                        WHERE
                            change_id = @Id 
                        RETURNING * ";

                await using (var deletedChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedChangeCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedChangeCommand = await deletedChangeCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedChangeCommand.ReadAsync())
                        {
                            var deleteEntity = MapReaderToChange(readerDeletedChangeCommand);

                            _logger.LogInformation("Запис зміни {Id} успішно видалено. Дата видалення: {Now}",
                                deleteEntity.Id,
                                DateTime.UtcNow);

                            return deleteEntity;
                        }
                    }
                }

                throw new ReturningNoDataException("ЗАпису не знайдено, видалення не можливе");
            });
        }

        public async Task<UserPendingChange> UpdateAsync(UserPendingChange entity)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        UPDATE 
                            User_Changes 
                        SET 
                            user_id = @UserId,
                            confirmation_token = @ConfirmToken,
                            change_type = @ChangeType, 
                            new_password_hash = @NewPasswordHash,
                            new_login = @NewLogin,
                            expires_at = @ExpiresAt,
                            is_confirmed = @IsConfirmed
                        WHERE 
                            change_id = @Id
                        RETURNING * ";

                object DbValue(object? value) => value ?? DBNull.Value;

                await using (var updateChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    updateChangeCommand.Parameters.AddWithValue("@Id", entity.Id);
                    updateChangeCommand.Parameters.AddWithValue("@UserId", entity.UserId);
                    updateChangeCommand.Parameters.AddWithValue("@ConfirmToken", entity.ConfirmationToken.ToString());
                    updateChangeCommand.Parameters.AddWithValue("@ChangeType", entity.UserChangeType.ToString());
                    updateChangeCommand.Parameters.AddWithValue("@NewPasswordHash", DbValue(entity.NewPasswordHash));
                    updateChangeCommand.Parameters.AddWithValue("@NewLogin", DbValue(entity.NewLogin));
                    updateChangeCommand.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
                    updateChangeCommand.Parameters.AddWithValue("@IsConfirmed", entity.IsConfirmed);

                    await using (var readerChangeCommand = await updateChangeCommand.ExecuteReaderAsync())
                    {
                        if (await readerChangeCommand.ReadAsync())
                        {
                            var updateChange = MapReaderToChange(readerChangeCommand);

                            _logger.LogInformation("Дані оновлені {Id} успішно оновлені. Дата оновлення: {Now}",
                                updateChange.Id,
                                DateTime.UtcNow);

                            return updateChange;
                        }
                    }
                }

                throw new ReturningNoDataException("Не вдалося отримати дані при оновленні змін");
            });
        }

        public async Task<UserPendingChange?> GetPendingChangesAsync(Guid id)
        {
            return await ExecuteAsync(async () =>
            {
                await using var conn = await _dbConnectionProvider.GetOpenConnectionAsync();

                var sql = @"
                        SELECT 
                            change_id, user_id, confirmation_token, change_type, new_password_hash, new_login, created_at, expires_at, is_confirmed 
                        FROM 
                            User_Changes 
                        WHERE 
                            change_id = @Id";

                await using (var getChangeByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getChangeByIdCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerGeChangeByIdCommand = await getChangeByIdCommand.ExecuteReaderAsync())
                    {
                        if (await readerGeChangeByIdCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToChange(readerGeChangeByIdCommand);

                            return userEntity;
                        }
                    }
                }

                return null;
            });
        }

        private UserPendingChange MapReaderToChange(NpgsqlDataReader reader)
        {
            return new UserPendingChange()
            {
                Id = reader.GetGuid(reader.GetOrdinal("change_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                ConfirmationToken = reader["confirmation_token"] as string ?? string.Empty,
                UserChangeType = Enum.TryParse<UserChangeType>(reader["change_type"]?.ToString(), out var type) ? type : UserChangeType.PasswordChange,
                NewPasswordHash = reader["new_password_hash"] as string ?? string.Empty,
                NewLogin = reader["new_login"] as string ?? string.Empty,
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at")),
                IsConfirmed = Convert.ToBoolean(reader["is_confirmed"]),
            };
        }
    }
}
