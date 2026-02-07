using Authorization.DAL.Context.Interface;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;

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

        public Task<RepositoryResponse<UserRestriction>> CreateAsync(UserRestriction entity)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserRestriction>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
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

        public Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetAllBansByUserIdAsync(Guid idUser)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<UserRestriction>> GetBanByIdAsync(Guid idBan)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetBansByAdminNicknameAsync(string adminNickname)
        {
            throw new NotImplementedException();
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
