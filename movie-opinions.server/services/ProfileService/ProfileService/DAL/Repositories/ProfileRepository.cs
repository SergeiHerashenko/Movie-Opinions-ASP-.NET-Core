using ProfileService.DAL.Connect_Database;
using ProfileService.DAL.Interface;
using ProfileService.Models.Profile;
using ProfileService.Models.Responses;

namespace ProfileService.DAL.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IConnectProfileDb _connectProfileDb;

        public ProfileRepository(IConnectProfileDb connectProfileDb)
        {
            _connectProfileDb = connectProfileDb;
        }

        public async Task<RepositoryResult<bool>> CreateUserAsync(UserProfile profileUser)
        {
            // Затичка для тестування !
            //return new RepositoryResult<bool>
            //{
            //    StatusCode = Models.Enums.ProfileStatusCode.ProfileCreated,
            //    IsSuccess = true,
            //};
            throw new NotImplementedException();
        }

        public Task<RepositoryResult<bool>> DeleteUserAsync(Guid userId)
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
