using Contracts.Interfaces;
using Profile.Domain.Entities;

namespace Profile.Application.Interfaces.Repositories
{
    public interface IUserProfileRepositories : IBaseRepository<UserProfile>
    {
        Task<UserProfile> GetByUserIdAsync(Guid userId);
    }
}
