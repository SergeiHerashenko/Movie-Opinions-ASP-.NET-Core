using Authorization.Application.Interfaces.Cookie;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.DAL.Interface;
using Authorization.Domain.DTO;
using Authorization.Domain.Request;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly ICookieProvider _cookieProvider;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AccountService> _logger;

        public AccountService(ICookieProvider cookieProvider,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ILogger<AccountService> logger)
        {
            _cookieProvider = cookieProvider;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<ServiceResponse> InitiatePasswordChangeAsync(ChangePasswordModel model)
        {
            _logger.LogInformation("Отримання id користувача");

            Guid userId = _cookieProvider.GetUserId();

            if(userId == Guid.Empty)
            {
                _logger.LogInformation("Помилка при отриманні Id користувача");

                return new ServiceResponse()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = "Помилка отримання ідентифікатора користувача"
                };
            }

            var userEntity = await _userRepository.GetUserByIdAsync(userId);

            if(userEntity.StatusCode != StatusCode.General.Ok)
            {
                return new ServiceResponse()
                {
                    IsSuccess = false,
                    StatusCode = userEntity.StatusCode,
                    Message = userEntity.Message
                };
            }

            _logger.LogInformation("Перевірка паролю користувача!");

            bool isPasswordCorrect = await _passwordHasher.VerifyPasswordAsync(
                model.OldPassword,
                userEntity.Data.PasswordSalt,
                userEntity.Data.PasswordHash
                );

            if (!isPasswordCorrect)
            {
                _logger.LogInformation("Невірний пароль!");

                return new ServiceResponse()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.Auth.Unauthorized,
                    Message = "Невірний пароль!"
                };
            }

            return new ServiceResponse()
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Пароль вірний!"
            };
        }

        public async Task<ServiceResponse> ChangePasswordAsync(string code, ChangePasswordModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string userEmail)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse> ResetPasswordAsync(string newPassword)
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
