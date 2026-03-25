using Authorization.Domain.Exceptions;

namespace Authorization.Domain.ValueObjects
{
    public class Password
    {
        public string Hash { get; }

        public Password(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new DomainException(DomainErrorCodes.InvalidPasswordHash, "Хеш пароля не може бути пустим");

            Hash = hash;
        }
    }
}
