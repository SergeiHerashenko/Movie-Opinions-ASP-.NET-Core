using Authorization.Domain.Entities;
using Contracts.Interfaces;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction>
    {
        Task<UserRestriction?> GetBanByIdAsync(Guid banId);

        Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId);

        Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId);

        Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname);
    }
}
