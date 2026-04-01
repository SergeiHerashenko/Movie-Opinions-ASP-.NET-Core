using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class UserPendingRegistrationRepository : IUserPendingRegistrationRepository
    {
        public Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration?> ExistsByRegistrationLoginAsync(Login login)
        {
            throw new NotImplementedException();
        }

        public Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity)
        {
            throw new NotImplementedException();
        }
    }
}
