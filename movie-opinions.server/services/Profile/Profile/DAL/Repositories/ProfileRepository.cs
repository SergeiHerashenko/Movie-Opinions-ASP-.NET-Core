using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Npgsql;
using Profile.DAL.Connect_Database;
using Profile.DAL.Interface;
using Profile.Models.Profile;

namespace Profile.DAL.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IConnectProfileDb _connectProfileDb;

        public ProfileRepository(IConnectProfileDb connectProfileDb)
        {
            _connectProfileDb = connectProfileDb;
        }

        public async Task<RepositoryResponse<UserProfile>> CreateAsync(UserProfile entity)
        {
            using (var conn = new NpgsqlConnection(_connectProfileDb.GetConnectProfileDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var createUser = new NpgsqlCommand(
                        "INSERT INTO " +
                            "Users_Profile_Table (id_user, user_name, first_name, last_name, phone_number, bio, avatar_url, created_at, last_updated_at) " +
                        "VALUES (@Id, @Name, @FirstName, @LastName, @PhoneNumber, @Bio, @AvatarUrl, NOW(), @LastUpdatedAt);", conn))
                    {
                        createUser.Parameters.AddWithValue("@Id", entity.UserId);
                        createUser.Parameters.AddWithValue("@Name", entity.UserName);
                        createUser.Parameters.AddWithValue("@FirstName", entity.FirstName ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@LastName", entity.LastName ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@PhoneNumber", entity.PhoneNumber ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@Bio", entity.Bio ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@AvatarUrl", entity.AvatarUrl ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@LastUpdatedAt", entity.LastUpdatedAt ?? (object)DBNull.Value);

                        await createUser.ExecuteNonQueryAsync();
                    }

                    return new RepositoryResponse<UserProfile>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCode.Create.Created,
                        Message = "Користувач створений!",
                        Data = entity
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<UserProfile>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Помилка в базі даних!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<UserProfile>> UpdateAsync(UserProfile entity)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse<UserProfile>> DeleteAsync(Guid id)
        {
            await using (var conn = new NpgsqlConnection(_connectProfileDb.GetConnectProfileDataBase()))
            {
                try
                {
                    await conn.OpenAsync();

                    await using (var deleteUser = new NpgsqlCommand(
                        "DELETE FROM " +
                            "Users_Profile_Table " +
                        "WHERE id_user = @ID RETURNING *", conn))
                    {
                        deleteUser.Parameters.AddWithValue("@ID", id);

                        await using (var readerInformationUser = await deleteUser.ExecuteReaderAsync())
                        {
                            if (await readerInformationUser.ReadAsync())
                            {
                                var userEntity = MapReaderToUser(readerInformationUser);

                                return new RepositoryResponse<UserProfile>()
                                {
                                    IsSuccess = true,
                                    StatusCode = StatusCode.General.Ok,
                                    Message = "Користувача видалено!",
                                    Data = userEntity
                                };
                            }
                        }
                    }

                    return new RepositoryResponse<UserProfile>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCode.Delete.Ok,
                        Message = "Користувача видалено!"
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResponse<UserProfile>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Помилка в базі даних!" + ex.Message
                    };
                }
            }
        }

        public async Task<RepositoryResponse<List<UserProfile>>> GetSearchUsersByNameAsync(string searchName)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse<UserProfile>> GetUserById(Guid userId)
        {
            throw new NotImplementedException();
        }

        private UserProfile MapReaderToUser(NpgsqlDataReader reader)
        {
            return new UserProfile()
            {
                UserId = Guid.Parse(reader["id_user"].ToString()),
                UserName = reader["user_name"].ToString(),
                FirstName = reader["first_name"].ToString(),
                LastName = reader["last_name"].ToString(),
                PhoneNumber = reader["phone_number"].ToString(),
                Bio = reader["bio"].ToString(),
                AvatarUrl = reader["avatar_url"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                LastUpdatedAt = Convert.ToDateTime(reader["last_updated_at"]),
            };
        }
    }
}
