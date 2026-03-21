using Authorization.Application.DTO.Users.Change;
using Contracts.Models.Response;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<Result<InitiatePasswordChangeResponse>> InitiatePasswordChangeAsync(InitiatePasswordChangeDTO initiatePasswordChangeDTO);

        Task<Result> SendVerificationCodeAsync(SendVerificationCodeDTO sendVerificationCodeDTO);

        Task<Result> ConfirmPasswordChangeAsync(PasswordConfirmationDTO passwordConfirmationDTO);

        Task<Result<ResetPasswordResponse>> ResetPasswordAsync(string login);

        Task<Result<string>> VerifyResetCodeAsync(VerifyResetDTO verifyResetDTO);

        Task<Result> FinalizePasswordResetAsync(FinalizePasswordResetDTO finalizePasswordResetDTO);
    }
}
