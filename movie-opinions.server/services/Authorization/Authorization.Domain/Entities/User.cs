using Authorization.Domain.Common;
using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Authorization.Domain.Exceptions.DomainErrorCode;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities
{
    public class User : BaseEntity
    {
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public string? LastLoginIp { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedLoginAttempts { get; private set; }

        public bool IsBlocked { get; private set; }

        public bool IsDeleted { get; private set; }

        private User(Login login, Password password, Role role, string? lastLoginIp) : base()
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = null;
            LastLoginAt = null;
            LastLoginIp = lastLoginIp;
            IsLoginConfirmed = true;
            FailedLoginAttempts = 0;
            IsBlocked = false;
            IsDeleted = false;
        }

        public User(Guid id, 
            DateTime createdAt, 
            Login login,
            Password password, 
            Role role, 
            DateTime? updateAt,
            DateTime? lastLoginAt,
            string? lastLoginIp,
            bool isConfirmed, 
            int failedLoginAttempts, 
            bool isBlocked, 
            bool isDeleted)
            :base(id, createdAt)
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = updateAt;
            LastLoginAt = lastLoginAt;
            LastLoginIp = lastLoginIp;
            IsLoginConfirmed = isConfirmed;
            FailedLoginAttempts = failedLoginAttempts;
            IsBlocked = isBlocked;
            IsDeleted = isDeleted;
        }

        public static User CreateNewUser(Login login, Password password, Role role, string? lastLoginIp)
        {
            return new User(login, password, role, lastLoginIp);
        }

        public void ChangeLogin(Login newLogin)
        {
            if (Login == newLogin) return;

            Login = newLogin;
            IsLoginConfirmed = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ConfirmLogin()
        {
            if (IsDeleted)
                throw new ForbiddenException(DomainErrorCodes.UserDeleted, "Неможливо підтвердити логін видаленого користувача.");

            if (IsLoginConfirmed) return;

            IsLoginConfirmed = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Block()
        {
            if (IsDeleted)
                throw new ForbiddenException(DomainErrorCodes.UserDeleted,"Неможливо заблокувати видаленого користувача.");

            if (IsBlocked) return;

            IsBlocked = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void FailedLogin()
        {
            if (IsBlocked || IsDeleted) return;

            FailedLoginAttempts++;
            UpdatedAt = DateTime.UtcNow;

            if(FailedLoginAttempts >= 3)
            {
                this.Block();

                throw new BadRequestException(DomainErrorCodes.TooManyLoginAttempts, "Акаунт заблоковано через велику кількість невдалих спроб входу.");
            }
        }

        public void LoginSuccess(string ip)
        {
            if(IsDeleted || IsBlocked)
                throw new ForbiddenException(DomainErrorCodes.UserDeleted, "Неможливо виконати вхід для заблокованого або видаленого користувача.");

            FailedLoginAttempts = 0;

            LastLoginAt = DateTime.UtcNow;
            LastLoginIp = ip;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            if (IsDeleted) return;

            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(Password newPassword)
        {
            if(IsDeleted)
                throw new ForbiddenException(DomainErrorCodes.UserDeleted, "Не можна змінити пароль видаленого користувача.");

            Password = newPassword;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
