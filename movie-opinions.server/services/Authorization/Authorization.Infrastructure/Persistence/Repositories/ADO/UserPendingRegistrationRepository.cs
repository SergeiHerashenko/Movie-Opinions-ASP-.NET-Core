using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class UserPendingRegistrationRepository : RepositoryBase, IUserPendingRegistrationRepository
    {
        public UserPendingRegistrationRepository(IDbConnectionProvider dbconnectionProvider,
            ILogger<AdoUserRepository> logger)
                : base(logger, dbconnectionProvider){ }

        public async Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        INSERT INTO 
                            Users_Pending_Registration (user_id, login_user, login_type, password_hash, created_at, expires_at)
                        VALUES 
                            (@Id, @Login,@LoginType, @PasswordHash, NOW(), @ExpiresAt)
                        RETURNING * ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerInsertUserCommand.ReadAsync())
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);

                            _logger.LogInformation("Користувач {Login} збережений в таблицю тимчасовио зареєстрованих. Guid {Id}. Дата створення: {Now}",
                                newUser.Login.Value,
                                newUser.Id,
                                DateTime.UtcNow);

                            return newUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingRegistration), entity.Id);
            });
        }

        public async Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        UPDATE 
                            Users_Pending_Registration 
                        SET 
                            login = @Login, 
                            login_type = @LoginType, 
                            password_hash = @PasswordHash, 
                            expires_at = @ExpiresAt 
                        WHERE
                            user_id = @Id 
                        RETURNING * ";

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateUserCommand, entity);

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerUpdateUserCommand.ReadAsync())
                        {
                            var updateUser = MapReaderToUser(readerUpdateUserCommand);

                            _logger.LogInformation("Дані користувача {Login} успішно оновлені. Guid {Id}. Дата оновлення: {Now}",
                                updateUser.Login.Value,
                                updateUser.Id,
                                DateTime.UtcNow);

                            return updateUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingRegistration), entity.Id);
            });
        }

        public async Task<UserPendingRegistration> DeleteAsync(Guid id)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"
                        DELETE FROM 
                            Users_Pending_Registration 
                        WHERE 
                            user_id = @Id 
                        RETURNING * ";

                await using (var deleteUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedUserCommand = await deleteUserCommand.ExecuteReaderAsync())
                    {
                        if (await readerDeletedUserCommand.ReadAsync())
                        {
                            var userEntity = MapReaderToUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувача {Login} успішно видалено з таблиці реєстрації. Guid {Id}. Дата видалення: {Now}",
                                userEntity.Login.Value,
                                userEntity.Id,
                                DateTime.UtcNow);

                            return userEntity;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingRegistration), id);
            });
        }

        public async Task<bool> ExistsByRegistrationLoginAsync(Login login)
        {
            return await ExecuteWithConnectionAsync(async (conn) =>
            {
                var sql = @"SELECT EXISTS(SELECT 1 FROM Users_Pending_Registration WHERE login = @Login)";

                await using (var existsUserCommand = new NpgsqlCommand(sql, conn))
                {
                    existsUserCommand.Parameters.AddWithValue("@Login", login.Value);

                    var result = await existsUserCommand.ExecuteScalarAsync();

                    return result is bool exists && exists;
                }
            });
        }

        private UserPendingRegistration MapReaderToUser(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("user_id");

            var loginValue = reader.GetString("login");
            if (!Enum.TryParse<LoginType>(reader.GetString("login_type"), out var loginType))
                throw new DataConsistencyException("Invalid LoginType in DB");

            var login = Login.Restore(loginValue, loginType);

            var passwordHash = new Password(reader.GetString("password_hash"));

            var createdAt = reader.GetDateTime("created_at");
            var expiresAt = reader.GetDateTime("expires_at");

            return new UserPendingRegistration(id, login, passwordHash, createdAt, expiresAt);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserPendingRegistration entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@Login", entity.Login.Value);
            cmd.Parameters.AddWithValue("@LoginType", entity.Login.Type);
            cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash.Hash);
            cmd.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
        }
    }
}
