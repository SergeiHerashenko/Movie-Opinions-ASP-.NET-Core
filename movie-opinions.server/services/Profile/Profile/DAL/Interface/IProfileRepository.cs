using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;
using Profile.Models.Profile;

namespace Profile.DAL.Interface
{
    public interface IProfileRepository : IBaseRepository<UserProfile, RepositoryResponse<UserProfile>>
    {
        Task<RepositoryResponse<UserProfile>> GetUserById(Guid userId);

        Task<RepositoryResponse<List<UserProfile>>> GetSearchUsersByNameAsync(string searchName);
    }
}
