using Authorization.Application.DTO.Users.Change;
using Authorization.Application.DTO.Users.Response;
using Authorization.Application.Interfaces.Services;

namespace Authorization.Application.Services
{
    public class AccountService : IAccountService
    {
        public Task<ChangeResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync()
        {
            throw new NotImplementedException();
        }

        public Task ConfirmEmailChangeAsync()
        {
            throw new NotImplementedException();
        }

        public Task RequestEmailChangeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
