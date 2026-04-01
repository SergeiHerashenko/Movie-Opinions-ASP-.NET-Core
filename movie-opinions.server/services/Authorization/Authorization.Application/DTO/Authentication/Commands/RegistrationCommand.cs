namespace Authorization.Application.DTO.Authentication.Commands
{
    public class RegistrationCommand
    {
        public required string Login { get; init; }

        public required string Password { get; init; }
    }
}
