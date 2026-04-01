using Authorization.Domain.Exceptions;
using Authorization.Domain.Exceptions.DomainErrorCode;

namespace Authorization.Domain.ValueObjects
{
    public class Password
    {
        public string Hash { get; }

        public Password(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new BadRequestException(DomainErrorCodes.InvalidPasswordHash, "Хеш пароля не може бути пустим");

            Hash = hash;
        }
    }
}
