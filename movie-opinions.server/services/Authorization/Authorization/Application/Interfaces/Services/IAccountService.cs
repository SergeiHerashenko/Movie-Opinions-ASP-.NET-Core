using Authorization.Domain.Request;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<ServiceResponse<string>> InitiateAccountChange(ChangePasswordModel model);





        Task<ServiceResponse> ChangePasswordAsync(string code, ChangePasswordModel model);

        Task<ServiceResponse> ForgotPasswordAsync(string userEmail);

        Task<ServiceResponse> ResetPasswordAsync(string newPassword);

        Task<ServiceResponse> ChangeEmailAsync(ChangeEmailModel model);

        Task<ServiceResponse> SendingConfirmationAsync(SendVerificationCodeRequest request);
    }
}
