namespace Authorization.Domain.Exceptions
{
    public static class DomainErrorCodes
    {
        public const string UserNotFound = "USER_NOT_FOUND";

        public const string UserBlocked = "USER_BLOCKED";

        public const string UserDeleted = "USER_DELETED";

        public const string InvalidLogin = "INVALID_LOGIN";

        public const string InvalidPasswordHash = "INVALID_PASSWORD_HASH";

        public const string LoginNotConfirmed = "LOGIN_NOT_CONFIRMED";

        public const string TooManyLoginAttempts = "TOO_MANY_LOGIN_ATTEMPTS";

        public const string LoginAlreadyInUse = "LOGIN_ALREADY_IN_USE";

        public const string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
    }
}
