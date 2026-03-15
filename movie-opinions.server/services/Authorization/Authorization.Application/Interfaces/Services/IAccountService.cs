using Authorization.Application.DTO.Users.Change;
using Contracts.Models.Response;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<Result<InitiatePasswordChangeResponse>> InitiatePasswordChangeAsync(InitiatePasswordChangeDTO initiatePasswordChangeDTO);

        Task<Result> SendVerificationCodeAsync(SendVerificationCodeDTO sendVerificationCodeDTO);

        Task<Result> ConfirmPasswordChangeAsync(PasswordConfirmationDTO passwordConfirmationDTO);
    }
}
