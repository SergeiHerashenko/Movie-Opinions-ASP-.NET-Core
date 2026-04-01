using Authorization.Application.Common;
using Authorization.Application.DTO.Authentication.Commands;
using Authorization.Application.DTO.Authentication.Results;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        Task<Result> LoginAsunc(LoginCommand loginCommand);

        Task<Result<RegistrationResult>> RegistrationAsync(RegistrationCommand registrationCommand);

        Task<Result> LogoutAsync();

        Task<Result> RefreshSessionAsync();
    }
}
