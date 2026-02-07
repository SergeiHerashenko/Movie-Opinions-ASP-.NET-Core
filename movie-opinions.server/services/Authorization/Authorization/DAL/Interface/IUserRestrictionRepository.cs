using Authorization.Domain.Entities;
using MovieOpinions.Contracts.Models.Interface;
using MovieOpinions.Contracts.Models.RepositoryResponse;

namespace Authorization.DAL.Interface
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction, RepositoryResponse<UserRestriction>>
    {
        Task<RepositoryResponse<UserRestriction>> GetActiveBanByUserIdAsync(Guid idUser);

        Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetAllBansByUserIdAsync(Guid idUser);

        Task<RepositoryResponse<UserRestriction>> GetBanByIdAsync(Guid idBan);

        Task<RepositoryResponse<IEnumerable<UserRestriction>>> GetBansByAdminNicknameAsync(string adminNickname);
    }
}
