using Authorization.Application.DTO.Users.Change;
using Authorization.Application.DTO.Users.Response;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<ChangeResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);

        Task ResetPasswordAsync();

        Task RequestEmailChangeAsync();

        Task ConfirmEmailChangeAsync();
    }
}
