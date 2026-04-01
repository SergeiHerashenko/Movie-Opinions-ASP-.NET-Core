using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Authorization.Domain.Exceptions.DomainErrorCode;

namespace Authorization.Domain.ValueObjects
{
    public class Login
    {
        public string Value { get; }

        public LoginType Type { get; }

        private Login(string value, LoginType type)
        {
            if (string.IsNullOrEmpty(value))
                throw new BadRequestException(DomainErrorCodes.InvalidLogin, "Логін не може бути пустим");

            if (type == LoginType.Login_Email && !value.Contains("@"))
                throw new BadRequestException(DomainErrorCodes.InvalidLogin, "Для Email логін має містити @");

            if (type == LoginType.Login_Phone && value.Any(char.IsLetter))
                throw new BadRequestException(DomainErrorCodes.InvalidLogin, "Телефон не може містити літери");

            Value = value;
            Type = type;
        }

        public static Login Create(string rawLogin)
        {
            if (string.IsNullOrEmpty(rawLogin))
                throw new BadRequestException(DomainErrorCodes.InvalidLogin, "Логін не може бути порожнім");

            LoginType type = rawLogin switch
            {
                var s when s.Contains("@") => LoginType.Login_Email,

                var s when long.TryParse(s.Replace("+", ""), out _) => LoginType.Login_Phone,

                _ => throw new BadRequestException(DomainErrorCodes.InvalidLogin, "Невідомий формат логіна (очікується Email або Телефон)")
            };

            return new Login(rawLogin, type);
        }

        public static Login Restore(string value, LoginType loginType)
        {
            return new Login(value, loginType);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Login other) return false;
            return Value == other.Value && Type == other.Type;
        }

        public override int GetHashCode() => HashCode.Combine(Value, Type);

        public static bool operator ==(Login? a, Login? b) => Equals(a, b);
        public static bool operator !=(Login? a, Login? b) => !Equals(a, b);
    }
}
