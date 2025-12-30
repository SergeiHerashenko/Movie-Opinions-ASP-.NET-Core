using Npgsql;
using Profile.DAL.Connect_Database;
using Profile.DAL.Interface;
using Profile.Models.Profile;
using Profile.Models.Responses;

namespace Profile.DAL.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IConnectProfileDb _connectProfileDb;

        public ProfileRepository(IConnectProfileDb connectProfileDb)
        {
            _connectProfileDb = connectProfileDb;
        }

        public async Task<RepositoryResult<Guid>> CreateUserAsync(UserProfile profileUser)
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
                        createUser.Parameters.AddWithValue("@Id", profileUser.UserId);
                        createUser.Parameters.AddWithValue("@Name", profileUser.UserName);
                        createUser.Parameters.AddWithValue("@FirstName", profileUser.FirstName ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@LastName", profileUser.LastName ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@PhoneNumber", profileUser.PhoneNumber ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@Bio", profileUser.Bio ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@AvatarUrl", profileUser.AvatarUrl ?? (object)DBNull.Value);
                        createUser.Parameters.AddWithValue("@LastUpdatedAt", profileUser.LastUpdatedAt ?? (object)DBNull.Value);

                        await createUser.ExecuteNonQueryAsync();
                    }

                    return new RepositoryResult<Guid>
                    {
                        IsSuccess = true,
                        StatusCode = Models.Enums.ProfileStatusCode.ProfileCreated,
                        Data = profileUser.UserId
                    };
                }
                catch (Exception ex)
                {
                    return new RepositoryResult<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = Models.Enums.ProfileStatusCode.ProfileInternalError,
                        ErrorMessage = ex.Message,
                        Data = profileUser.UserId
                    };
                }
            }
        }

        public Task<RepositoryResult<Guid>> DeleteUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResult<List<UserSearchDTO>>> GetSearchUsersByNameAsync(string searchName)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResult<UserProfileDTO>> GetUserById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResult<UserProfileDTO>> UpdateUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
