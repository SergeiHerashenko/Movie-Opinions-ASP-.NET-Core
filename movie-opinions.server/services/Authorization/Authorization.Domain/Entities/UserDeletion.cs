using Authorization.Domain.Common;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Entities
{
    public class UserDeletion : BaseEntity
    {
        public Guid UserId { get; private set; }

        public string Login {  get; private set; }

        public string? Reason { get; private set; }

        private UserDeletion(Guid userId, string login, string? reason) : base()
        {
            if (userId == Guid.Empty)
                throw new DomainException(DomainErrorCodes.OperationNotAllowed, "Помилка отримання ідентифікатора користувача!");

            UserId = userId;
            Login = login;
            Reason = reason;
        }

        internal UserDeletion(Guid id, Guid userId, string login, string? reason, DateTime createdAt)
            :base(id, createdAt)
        {
            UserId = userId;
            Login = login;
            Reason = reason;
        }

        public static UserDeletion CreateForDeletedUser(User user, string? reason)
        {
            if (user.IsDeleted)
                throw new DomainException(DomainErrorCodes.UserDeleted, "Користувач вже видалений.");

            return new UserDeletion(user.Id, user.Login.Value, reason);
        }
    }
}
