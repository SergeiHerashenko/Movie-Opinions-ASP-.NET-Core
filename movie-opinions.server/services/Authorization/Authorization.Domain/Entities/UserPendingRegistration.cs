using Authorization.Domain.Common;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities
{
    public class UserPendingRegistration : BaseEntity
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(12);

        public Login Login { get; private set; }

        public Password PasswordHash { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        private UserPendingRegistration(Login login, Password passwordhash)
            : base()
        {
            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = DateTime.UtcNow.Add(ExpirationTime);
        }

        public UserPendingRegistration(Guid id, Login login, Password passwordhash, DateTime createdAt, DateTime expiresAt)
            : base(id, createdAt)
        {
            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Create(Login login, Password passwordHash)
        {
            return new UserPendingRegistration(login, passwordHash);
        }

        public void Refresh(Password passwordHash)
        {
            PasswordHash = passwordHash;
            ExpiresAt = DateTime.UtcNow.Add(ExpirationTime);
        }

        public bool IsExpired()
            => DateTime.UtcNow > ExpiresAt;
    }
}
