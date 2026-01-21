using MovieOpinions.Contracts.Models.RepositoryResponse;
using Profile.Models.Profile;

namespace Profile.DAL.Interface
{
    public interface IProfileRepository
    {
        Task<RepositoryResponse<Guid>> CreateUserAsync(UserProfile profileUser);

        Task<RepositoryResponse<Guid>> DeleteUserAsync(Guid userId);

        Task<RepositoryResponse<UserProfileDTO>> UpdateUserAsync(Guid userId);

        Task<RepositoryResponse<UserProfileDTO>> GetUserById(Guid userId);

        Task<RepositoryResponse<List<UserSearchDTO>>> GetSearchUsersByNameAsync(string searchName);
    }
}
