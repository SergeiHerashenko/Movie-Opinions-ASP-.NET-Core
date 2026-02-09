using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Request;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AccountService : IAccountService
    {
        public async Task<ServiceResponse> InitiatePasswordChangeAsync(ChangePasswordModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ChangePasswordAsync(string code, ChangePasswordModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string userEmail)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ResetPasswordAsunc(string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ChangeEmailAsync(ChangeEmailModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> SendingConfirmationAsync()
        {
            throw new NotImplementedException();
        }
    }
}
