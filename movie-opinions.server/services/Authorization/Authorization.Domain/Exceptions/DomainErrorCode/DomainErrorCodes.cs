namespace Authorization.Domain.Exceptions.DomainErrorCode
{
    public static class DomainErrorCodes
    {
        public const string InvalidPasswordHash = "INVALID_PASSWORD_HASH";

        public const string InvalidLogin = "INVALID_LOGIN";

        public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";

        public const string UserBlocked = "USER_BLOCKED";

        public const string UserDeleted = "USER_DELETED";

        public const string TooManyLoginAttempts = "TOO_MANY_LOGIN_ATTEMPTS";

        public const string TokenInvalid = "TOKEN_INVALID";

        public const string TokenExpired = "TOKEN_EXPIRED";
    }
}
