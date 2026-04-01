namespace Authorization.Application.Common
{
    public enum ErrorCode
    {
        None = 0,
        InternalServerError = 1,

        // User
        UserNotFound,
        UserDeleted,
        UserBlocked,
        UserAlreadyExists,

        InvalidCredentials,

        TokenNotFound,
        TokenExpired,
        TokenInvalid,

        EmailNotConfirmed,
        TooManyAttempts
    }
}
