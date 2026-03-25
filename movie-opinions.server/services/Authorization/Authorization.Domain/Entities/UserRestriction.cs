using Authorization.Domain.Common;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Entities
{
    public class UserRestriction : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Login {  get; private set; }

        public string? Reason { get; private set; }

        public string NameBannedBy { get; private set; }

        public DateTime? ExpiresAt { get; private set; }

        public bool IsActive { get; private set; }

        private UserRestriction(Guid userId, string login, string? reason, string nameBannedBy, DateTime? expiresAt)
            : base()
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = true;
        }

        internal UserRestriction(Guid id,
            Guid userId,
            string login,
            string? reason,
            string nameBannedBy,
            DateTime createdAt,
            DateTime? expiresAt,
            bool isActive)
            : base(id, createdAt)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
            NameBannedBy = nameBannedBy;
            ExpiresAt = expiresAt;
            IsActive = isActive;
        }

        public static UserRestriction CreateRestrictionForUser(User user, string? reason, string nameBannedBy, DateTime? expiresAt)
        {
            if (user.IsBlocked)
                throw new DomainException(DomainErrorCodes.UserBlocked, "Користувач вже має активне блокування!");

            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new DomainException(DomainErrorCodes.OperationNotAllowed, "Дата закінчення обмеження має бути в майбутньому.");

            return new UserRestriction(user.Id, user.Login.Value, reason, nameBannedBy, expiresAt);
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value;
        public bool IsCurrentlyActive => IsActive && !IsExpired;
    }
}
