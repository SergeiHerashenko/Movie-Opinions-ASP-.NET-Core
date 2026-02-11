using Authorization.Application.Interfaces.Cookie;
using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
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
        private readonly IUserPendingAccountChangesRepository _userPendingAccountChangesRepository;
        private readonly ISendInternalRequest _sendInternalRequest;

        public AccountService(ICookieProvider cookieProvider,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ILogger<AccountService> logger,
            IUserPendingAccountChangesRepository userPendingAccountChanges,
            ISendInternalRequest sendInternalRequest)
        {
            _cookieProvider = cookieProvider;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _userPendingAccountChangesRepository = userPendingAccountChanges;
            _sendInternalRequest = sendInternalRequest;
        }

        public async Task<ServiceResponse<string>> InitiateAccountChange(ChangePasswordModel model)
        {
            _logger.LogInformation("Старт перевірки користувача!");

            try
            {
                Guid userId = _cookieProvider.GetUserId();

                if (userId == Guid.Empty)
                {
                    _logger.LogInformation("Помилка при отриманні Id користувача");

                    return new ServiceResponse<string>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Помилка отримання ідентифікатора користувача"
                    };
                }

                var userEntity = await _userRepository.GetUserByIdAsync(userId);

                if (userEntity.StatusCode != StatusCode.General.Ok)
                {
                    return new ServiceResponse<string>()
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

                    return new ServiceResponse<string>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.Auth.Unauthorized,
                        Message = "Невірний пароль!"
                    };
                }

                _logger.LogInformation("Пароль підтверджено. Генерація запиту на зміну.");

                var createEntyti = await CreateEntityAsync(model, userId);

                var saveNewEntity = await _userPendingAccountChangesRepository.CreateAsync(createEntyti);

                if (saveNewEntity.StatusCode != StatusCode.Create.Created)
                {
                    _logger.LogWarning("Помилка запису в базу даних!");

                    return new ServiceResponse<string>()
                    {
                        IsSuccess = false,
                        StatusCode = saveNewEntity.StatusCode,
                        Message = saveNewEntity.Message
                    };
                }

                return new ServiceResponse<string>()
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Пароль вірний!",
                    Data = saveNewEntity.Data.ConfirmationToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Критична помилка системи");

                return new ServiceResponse<string>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична проблема системи!"
                };
            }
        }

        public async Task<ServiceResponse> SendingConfirmationAsync(SendVerificationCodeRequest request)
        {
            _logger.LogInformation("Відправка повідомлення для підтвердження дії!");

            try
            {
                if(request.ConfirmationToken == null)
                {
                    return new ServiceResponse()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.NotFound,
                        Message = "Не валідний токен підтвердження"
                    };
                }

                var existingEntity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(request.ConfirmationToken);

                if(existingEntity.StatusCode != StatusCode.General.Ok)
                {
                    return new ServiceResponse()
                    {
                        IsSuccess = false,
                        StatusCode = existingEntity.StatusCode,
                        Message = existingEntity.Message
                    };
                }

                Guid userId = _cookieProvider.GetUserId();

                if(userId != existingEntity.Data?.UserId)
                {
                    return new ServiceResponse()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.Auth.Unauthorized,
                        Message = "Помилка ідентифікації користувача!"
                    };
                }

                // Продовжити: Взяти з мікросервісу контактів (емейл телефон ) по ід юзера і відправити все в мікросервіс сповіщень!
            }
            catch (Exception ex)
            {

            }
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

        private async Task<UserPendingChanges> CreateEntityAsync(ChangePasswordModel entity, Guid userId)
        {
            string confirmationToken = GenerateToken();

            string passwordSalt = Guid.NewGuid().ToString();

            string passwordHash = await _passwordHasher.HashPasswordAsync(entity.NewPassword, passwordSalt);

            return new UserPendingChanges()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConfirmationToken = confirmationToken,
                ChangeType = Domain.Enum.UserChangeType.Password,
                NewPasswordHash = passwordHash,
                NewPasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20),
                IsConfirmed = false
            };
        }

        private string GenerateToken()
        {
            var randomNumber = new byte[64];

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}
