using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserPendingRegistrationRepository : IBaseRepository<UserPendingRegistration>
    {
        Task<UserPendingRegistration?> GetByLoginAsync(Login login);
    }
}
