using Authorization.Domain.Common;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities
{
    public class UserRefreshToken : BaseEntity
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(7);

        public Guid UserId { get; private set; }

        public string RefreshToken { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public string IpAddress { get; private set; }

        public string? City { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsUsed { get; private set; }

        public bool IsRevoked { get; private set; }

        private UserRefreshToken(Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city)
            : base()
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = DateTime.UtcNow.Add(ExpirationTime);
            IsUsed = false;
            IsRevoked = false;
        }

        internal UserRefreshToken(Guid id,
            Guid userId,
            string refreshToken,
            DeviceInfo deviceInfo,
            string ipAddress,
            string? city,
            DateTime expiresAt,
            DateTime createdAt,
            bool isUsed,
            bool isRevoked)
            : base(id, createdAt)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            ExpiresAt = expiresAt;
            IsUsed = isUsed;
            IsRevoked = isRevoked;
        }

        public static UserRefreshToken CreateRefreshToken(Guid userId, string refreshToken, DeviceInfo deviceInfo, string ipAddress, string? city)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new DomainException(DomainErrorCodes.OperationNotAllowed, "Помилка отримування токену");

            if (userId == Guid.Empty)
                throw new DomainException(DomainErrorCodes.OperationNotAllowed, "Помилка ідентифікації користувача!");

            return new UserRefreshToken(userId, refreshToken, deviceInfo, ipAddress, city);
        }

        public void Revoke()
        {
            if (IsRevoked) return;
            IsRevoked = true;
        }

        public void Use()
        {
            if (!IsActive)
                throw new DomainException(DomainErrorCodes.OperationNotAllowed, "Токен вже недійсний");
            IsUsed = true;
        }

        public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
