using Authorization.Domain.Common;
using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Authorization.Domain.Exceptions.DomainErrorCode;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities
{
    public class UserPendingChange : BaseEntity
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(30);

        public Guid UserId { get; private set; }

        public string ConfirmationToken { get; private set; }

        public UserChangeType UserChangeType { get; private set; }

        public Password? NewPassword { get; private set; }

        public Login? NewLogin { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsConfirmed { get; private set; }

        private UserPendingChange(Guid userId, 
            string confirmationToken, 
            UserChangeType userChangeType, 
            Password? newPassword, 
            Login? newLogin)
            : base()
        {
            UserId = userId;
            ConfirmationToken = confirmationToken;
            UserChangeType = userChangeType;
            NewPassword = newPassword;
            NewLogin = newLogin;
            ExpiresAt = DateTime.UtcNow.Add(ExpirationTime);
            IsConfirmed = false;
        }

        public UserPendingChange(Guid id,
            Guid userId,
            string confirmationToken,
            UserChangeType userChangeType,
            Password? newPassword,
            Login? newLogin,
            DateTime expiresAt,
            bool isConfirmed,
            DateTime createdAt)
            : base(id, createdAt)
        {
            UserId = userId;
            ConfirmationToken = confirmationToken;
            UserChangeType = userChangeType;
            NewPassword = newPassword;
            NewLogin = newLogin;
            ExpiresAt = expiresAt;
            IsConfirmed = isConfirmed;
        }

        private static void CheckValidation(Guid userId, string confirmationToken)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(confirmationToken))
                throw new BadRequestException(DomainErrorCodes.OperationNotAllowed, "Невалідний ідентифікатор користувача або помилка токену підтвердження");
        }

        public static UserPendingChange CreateLoginChange(Guid userId,
            string confirmationToken,
            Login newLogin)
        {
            CheckValidation(userId, confirmationToken);

            return new UserPendingChange(userId, confirmationToken, UserChangeType.LoginChange, null, newLogin);
        }

        public static UserPendingChange CreatePasswordChange(Guid userId,
            string confirmationToken,
            Password newPassword)
        {
            CheckValidation(userId, confirmationToken);

            return new UserPendingChange(userId, confirmationToken, UserChangeType.PasswordChange, newPassword, null);
        }

        public bool CanBeConfirmed(string token)
        {
            return !IsConfirmed &&
                   ConfirmationToken == token &&
                   ExpiresAt > DateTime.UtcNow;
        }

        public void Confirm(string token)
        {
            if (!CanBeConfirmed(token))
                throw new BadRequestException(DomainErrorCodes.OperationNotAllowed, "Токен недійсний, прострочений або вже використаний.");

            IsConfirmed = true;
        }
    }
}
