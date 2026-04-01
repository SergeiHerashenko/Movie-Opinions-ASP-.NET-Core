namespace Authorization.Application.DTO.Authentication.Commands
{
    public class LoginCommand
    {
        public required string Login { get; init; }

        public required string Password { get; init; }
    }
}
