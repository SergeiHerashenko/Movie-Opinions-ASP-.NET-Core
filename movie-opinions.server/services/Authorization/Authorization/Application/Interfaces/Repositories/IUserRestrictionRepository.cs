using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction, RepositoryResponse<UserRestriction>>
    {
        Task<RepositoryResponse<UserRestriction>> GetBanByIdAsync(Guid banId);

        Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetAllBansByUserIdAsync(Guid userId);

        Task<RepositoryResponse<UserRestriction>> GetActiveBanByUserIdAsync(Guid userId);

        Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetBansByAdminNicknameAsync(string adminNickname);
    }
}
