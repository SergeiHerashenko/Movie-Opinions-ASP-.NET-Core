using Profile.Models.Profile;
using Profile.Models.Responses;

namespace Profile.DAL.Interface
{
    public interface IProfileRepository
    {
        Task<RepositoryResult<Guid>> CreateUserAsync(UserProfile profileUser);

        Task<RepositoryResult<Guid>> DeleteUserAsync(Guid userId);

        Task<RepositoryResult<UserProfileDTO>> UpdateUserAsync(Guid userId);

        Task<RepositoryResult<UserProfileDTO>> GetUserById(Guid userId);

        Task<RepositoryResult<List<UserSearchDTO>>> GetSearchUsersByNameAsync(string searchName);
    }
}
