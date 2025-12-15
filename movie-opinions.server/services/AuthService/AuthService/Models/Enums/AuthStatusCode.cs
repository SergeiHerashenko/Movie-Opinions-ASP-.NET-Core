namespace AuthService.Models.Enums
{
    public enum AuthStatusCode
    {
        Success = 1,
        InvalidCredentials = 2,
        UserNotFound = 3,
        UserIsBlocked = 4,
    }
}
