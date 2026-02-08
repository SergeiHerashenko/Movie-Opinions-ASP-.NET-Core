using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;
using XAct.Users;

namespace Authorization.DAL.Repositories
{
    public class UserRestrictionRepository : IUserRestrictionRepository
    {
        private readonly IDbConnectionProvider _dbConnection;
        private readonly ILogger<UserRestrictionRepository> _logger;

        public UserRestrictionRepository(IDbConnectionProvider dbConnection, ILogger<UserRestrictionRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<RepositoryResponse<UserRestriction>> CreateAsync(UserRestriction entity)
        {
            _logger.LogInformation("Створення запису про блокування!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var createRecord = new NpgsqlCommand(
                        "INSERT INTO " +
                            "User_Restrictions (restriction_id, user_id, reason, name_banned_by, created_at, expires_at, is_active) " +
                        "VALUES " +
                            "(@IdRestriction, @IdUser, @Reason, @NameBannedBy, NOW(), @ExpiresAt, @IsActive) " +
                        "RETURNING *", conn))
                    {
                        createRecord.Parameters.AddWithValue("@IdRestriction", entity.Id);
                        createRecord.Parameters.AddWithValue("@IdUser", entity.UserId);
                        createRecord.Parameters.AddWithValue("@Reason", entity.Reason);
                        createRecord.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                        createRecord.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt ?? (object)DBNull.Value);
                        createRecord.Parameters.AddWithValue("@IsActive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;

                        await using (var readerCreatedRecord = await createRecord.ExecuteReaderAsync())
                        {
                            if (await readerCreatedRecord.ReadAsync())
                            {
                                var newRecord = MapReaderToBan(readerCreatedRecord);

                                _logger.LogInformation("Запис {Id} збережений в базу!", entity.Id);

                                return new RepositoryResponse<UserRestriction>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Create.Created,
                                    Data = newRecord,
                                    Message = "Запис створений!"
                                };
                            }
                        }
                    }

                    _logger.LogCritical("Сталась помилка запису!");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Сталась помилка запису!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Початок видалення запису!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deletedRecord = new NpgsqlCommand(
                        "DELETE FROM " +
                            "User_Restrictions " +
                        "WHERE " +
                            "restriction_id = @IdRestriction " +
                        "RETURNING *", conn))
                    {
                        deletedRecord.Parameters.AddWithValue("@IdRestriction", id);

                        await using (var readerInformationRecord = await deletedRecord.ExecuteReaderAsync())
                        {
                            if (await readerInformationRecord.ReadAsync())
                            {
                                var recodEntity = MapReaderToBan(readerInformationRecord);

                                _logger.LogInformation("Запис видалено!");

                                return new RepositoryResponse<UserRestriction>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Запис видалено!",
                                    Data = recodEntity
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Запис не знайдено, видалення не можливе");

                    return new RepositoryResponse<UserRestriction>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Запис не знайдено, видалення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserRestriction>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> UpdateAsync(UserRestriction entity)
        {
            _logger.LogInformation("Оновлення даних користувача");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var updateUser = new NpgsqlCommand(
                        "UPDATE " +
                            "User_Restrictions " +
                        "SET " +
                            "reason = @Reason " +
                            "name_banned_by = @NameBannedBy " +
                            "created_at = @CreatedAt " +
                            "expires_at = @ExpiresAt " +
                            "is_active = @IsActive " +
                        "WHERE " +
                            "user_id = @IdUser " +
                        "RETURNING * ", conn))
                    {
                        updateUser.Parameters.AddWithValue("@IdUser", NpgsqlTypes.NpgsqlDbType.Uuid).Value = entity.UserId;
                        updateUser.Parameters.AddWithValue("@Reason", entity.Reason);
                        updateUser.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
                        updateUser.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
                        updateUser.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
                        updateUser.Parameters.AddWithValue("@IsActive", entity.IsActive);

                        await using (var readerUpdateUser = await updateUser.ExecuteReaderAsync())
                        {
                            if (await readerUpdateUser.ReadAsync())
                            {
                                _logger.LogInformation("Інформація користувача оновлена");

                                return new RepositoryResponse<UserRestriction>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.Update.Ok,
                                    Data = MapReaderToBan(readerUpdateUser),
                                    Message = "Інформація успішно оновлена!"
                                };
                            }
                        }
                    }

                    _logger.LogWarning("Виникла помилка при оновленні інформації користувача!");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено, оновлення неможливе."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Критична помилка бази даних!");

                    return new RepositoryResponse<UserRestriction>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> GetActiveBanByUserIdAsync(Guid idUser)
        {
            _logger.LogInformation("Пошук активного бану користувача!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getBanned = new NpgsqlCommand(
                        "SELECT " +
                            "restriction_id, user_id, reason, name_banned_by, created_at, expires_at, is_active " +
                        "FROM " +
                            "User_Restrictions " +
                        "WHERE " +
                            "user_id = @IdUser " +
                        "AND " +
                            "is_active = true", conn))
                    {
                        getBanned.Parameters.AddWithValue("@IdUser", idUser);

                        await using (var readerBanned = await getBanned.ExecuteReaderAsync())
                        {
                            if (await readerBanned.ReadAsync())
                            {
                                var userRestriction = MapReaderToBan(readerBanned);

                                _logger.LogInformation("Заблокований користувач знайдено!");

                                return new RepositoryResponse<UserRestriction>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Заблокованого користувача знайдено!",
                                    Data = userRestriction
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Заблокованого користувача не знайдено");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Користувача не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetAllBansByUserIdAsync(Guid idUser)
        {
            _logger.LogInformation("Пошук всіх банів користувача!");

            var userRestrictionsList = new List<UserRestriction>();

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getBanneds = new NpgsqlCommand(
                        "SELECT " +
                            "restriction_id, user_id, reason, name_banned_by, created_at, expires_at, is_active " +
                        "FROM " +
                            "User_Restrictions " +
                        "WHERE " +
                            "user_id = @IdUser ", conn))
                    {
                        getBanneds.Parameters.AddWithValue("@IdUser", idUser);

                        await using (var readerBanneds = await getBanneds.ExecuteReaderAsync())
                        {
                            
                            while (await readerBanneds.ReadAsync())
                            {
                                var userRestriction = MapReaderToBan(readerBanneds);

                                userRestrictionsList.Add(userRestriction);
                            }

                            if (!userRestrictionsList.Any())
                            {
                                _logger.LogInformation("Записи користувача не знайдено!");

                                return new RepositoryResponse<IEnumerable<UserRestriction>>
                                {
                                    IsSuccess = false,
                                    Message = "У користувача немає зафіксованих обмежень",
                                    StatusCode = StatusCode.General.NotFound
                                };
                            }

                            _logger.LogInformation("Записи користувача знайдено!");

                            return new RepositoryResponse<IEnumerable<UserRestriction>>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Message = "Заблокованого користувача знайдено!",
                                Data = userRestrictionsList
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<IEnumerable<UserRestriction>>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserRestriction>> GetBanByIdAsync(Guid idBan)
        {
            _logger.LogInformation("Пошук бану за Id!");

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getBanned = new NpgsqlCommand(
                        "SELECT " +
                            "restriction_id, user_id, reason, name_banned_by, created_at, expires_at, is_active " +
                        "FROM " +
                            "User_Restrictions " +
                        "WHERE " +
                            "restriction_id = @IdRestriction ", conn))
                    {
                        getBanned.Parameters.AddWithValue("@IdRestriction", idBan);

                        await using (var readerBanned = await getBanned.ExecuteReaderAsync())
                        {
                            if (await readerBanned.ReadAsync())
                            {
                                var record = MapReaderToBan(readerBanned);

                                _logger.LogInformation("Запис бану знайдено!");

                                return new RepositoryResponse<UserRestriction>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Запис бану знайдено!",
                                    Data = record
                                };
                            }
                        }
                    }

                    _logger.LogInformation("Запис бану не знайдено");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Запис бану не знайдено!"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<UserRestriction>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        public async Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            _logger.LogInformation("Пошук всіх записів банів адміністратора!");

            var banRecordedByAdminList = new List<UserRestriction>();

            await using (var conn = new NpgsqlConnection(_dbConnection.GetConnectionString()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var getBanneds = new NpgsqlCommand(
                        "SELECT " +
                            "restriction_id, user_id, reason, name_banned_by, created_at, expires_at, is_active " +
                        "FROM " +
                            "User_Restrictions " +
                        "WHERE " +
                            "name_banned_by = @NameBannedBy ", conn))
                    {
                        getBanneds.Parameters.AddWithValue("@NameBannedBy", adminNickname);

                        await using (var readerBanneds = await getBanneds.ExecuteReaderAsync())
                        {

                            while (await readerBanneds.ReadAsync())
                            {
                                var banRecord = MapReaderToBan(readerBanneds);

                                banRecordedByAdminList.Add(banRecord);
                            }

                            if (!banRecordedByAdminList.Any())
                            {
                                _logger.LogInformation("Записів не знайдено!");

                                return new RepositoryResponse<IEnumerable<UserRestriction>>
                                {
                                    IsSuccess = false,
                                    Message = "У адміна немає записів!",
                                    StatusCode = StatusCode.General.NotFound
                                };
                            }

                            _logger.LogInformation("Записи банів адміна знайдено!");

                            return new RepositoryResponse<IEnumerable<UserRestriction>>()
                            {
                                IsSuccess = true,
                                StatusCode = StatusCode.General.Ok,
                                Message = "Записи банів адміна знайдено!",
                                Data = banRecordedByAdminList
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка баз даних!");

                    return new RepositoryResponse<IEnumerable<UserRestriction>>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Критична помилка баз даних!"
                    };
                }
            }
        }

        private UserRestriction MapReaderToBan(NpgsqlDataReader reader)
        {
            return new UserRestriction()
            {
                Id = reader.GetGuid(reader.GetOrdinal("restriction_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Reason = reader["reason"].ToString(),
                NameBannedBy = reader["name_banned_by"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                ExpiresAt = Convert.ToDateTime(reader["expires_at"]),
                IsActive = Convert.ToBoolean(reader["is_active"])
            };
        }
    }
}