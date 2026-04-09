using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction>
    {
        Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId);

        Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname);

        Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId);

        Task<UserRestriction?> GetBanByIdAsync(Guid banId);
    }
}
