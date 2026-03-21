using Authorization.Application.DTO.Integration;
using Authorization.Application.DTO.Integration.Responses;
using Authorization.Application.DTO.Users.Change;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Contracts.Integration;
using Contracts.Models.Response;
using Contracts.Models.Status;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHasher _hasher;
        private readonly IUserPendingAccountChangesRepository _userPendingAccountChangesRepository;
        private readonly IContactsSender _contactsSender;
        private readonly INotificationSender _notificationSender;
        private readonly IVerificationSender _verificationSender;
        private readonly IMaskContact _maskContact;
        private readonly IValidatorService _validatorService;
        private readonly IContactTypeDetector _contactTypeDetector;
        private readonly ITokenService _tokenService;
        private readonly IUserContext _userContext;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IUserRepository userRepository,
            IHasher hasher,
            IUserPendingAccountChangesRepository userPendingAccountChangesRepository,
            IContactsSender contactsSender,
            INotificationSender notificationSender,
            IVerificationSender verificationSender,
            IMaskContact maskContact,
            IValidatorService validatorService,
            IContactTypeDetector contactTypeDetector,
            ILogger<AccountService> logger,
            ITokenService tokenService,
            IUserContext userContext)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _userPendingAccountChangesRepository = userPendingAccountChangesRepository;
            _contactsSender = contactsSender;
            _notificationSender = notificationSender;
            _verificationSender = verificationSender;
            _maskContact = maskContact;
            _validatorService = validatorService;
            _contactTypeDetector = contactTypeDetector;
            _logger = logger;
            _tokenService = tokenService;
            _userContext = userContext;
        }

        public async Task<Result<InitiatePasswordChangeResponse>> InitiatePasswordChangeAsync(InitiatePasswordChangeDTO initiatePasswordChangeDTO)
        {
            var resultValidate = await _validatorService.ValidateForUser(initiatePasswordChangeDTO.OldPassword, initiatePasswordChangeDTO.NewPassword);

            if(!resultValidate.IsSuccess)
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = resultValidate.IsSuccess,
                    Message = resultValidate.Message,
                    StatusCode = resultValidate.StatusCode
                };
            }

            var contactsResponse = await _contactsSender.GetUserChannelsAsync(resultValidate.Data);

            if (!contactsResponse.IsSuccess)
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "Помилка при отриманні списку контактів!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var (createChangeEntity, token) = CreateEntity(resultValidate.Data, UserChangeType.PasswordChange, newPassword: initiatePasswordChangeDTO.NewPassword);

            var saveChangeEntity = await _userPendingAccountChangesRepository.CreateAsync(createChangeEntity);

            return new Result<InitiatePasswordChangeResponse>()
            {
                IsSuccess = true,
                Message = "Запит на зміну пароля створено!",
                StatusCode = StatusCode.General.Ok,

                Data = new InitiatePasswordChangeResponse()
                {
                    RequestId = saveChangeEntity.Id,
                    ConfirmationToken = token,
                    CommunicationChannel = contactsResponse.Data!.Select(c => new ContactResponseDTO
                    {
                        UserId = c.UserId,
                        ContactValue = _maskContact.MaskContactValue(c.ContactValue, c.CommunicationChannel),
                        CommunicationChannel = c.CommunicationChannel
                    }).ToList()
                }
            };
        }

        public async Task<Result> SendVerificationCodeAsync(SendVerificationCodeDTO sendVerificationCodeDTO)
        {
            var resultValidate = await _validatorService.ValidateForSend(sendVerificationCodeDTO.RequestId, sendVerificationCodeDTO.ConfirmationToken);

            if (!resultValidate.IsSuccess)
            {
                return new Result()
                {
                    IsSuccess = resultValidate.IsSuccess,
                    Message = resultValidate.Message,
                    StatusCode = resultValidate.StatusCode
                };
            }

            var userId = resultValidate.Data;

            var contactsResponse = await _contactsSender.GetUserChannelsAsync(userId);

            if (!contactsResponse.IsSuccess || contactsResponse.Data == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка при отриманні списку контактів!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var selectedContact = contactsResponse.Data
                .FirstOrDefault(c => c.CommunicationChannel == sendVerificationCodeDTO.CommunicationChannel);

            if (selectedContact == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Обраний канал зв'язку недоступний!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var notificationEntity = new NotificationIntegrationDTO()
            {
                UserId = userId,
                Recipient = selectedContact.ContactValue,
                Action = MessageActions.PasswordChange,
                Channel = sendVerificationCodeDTO.CommunicationChannel
            };

            var notificationResponse = await _notificationSender.SendCreateNotificationAsync(notificationEntity);

            if (!notificationResponse.IsSuccess)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка сервісу сповіщення!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            return new Result()
            {
                IsSuccess = true,
                Message = "Підтверження відправлено!",
                StatusCode = StatusCode.General.Ok
            };
        }

        public async Task<Result> ConfirmPasswordChangeAsync(PasswordConfirmationDTO passwordConfirmationDTO)
        {
            if (string.IsNullOrEmpty(passwordConfirmationDTO.Code))
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Недійсний код",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var resultValidate = await _validatorService.ValidateForConfirm(passwordConfirmationDTO.RequestId, passwordConfirmationDTO.ConfirmationToken);

            if (!resultValidate.IsSuccess || resultValidate.Data == null)
            {
                return new Result()
                {
                    IsSuccess = resultValidate.IsSuccess,
                    Message = resultValidate.Message,
                    StatusCode = resultValidate.StatusCode
                };
            }

            var code = await _verificationSender.GetCodeAsync(passwordConfirmationDTO.RequestId);

            if (!code.IsSuccess || code.Data == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка сервісу верифікації!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            if (!_hasher.Verify(code.Data, passwordConfirmationDTO.Code))
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Недійсний код",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var entityUser = await _userRepository.GetUserByIdAsync(resultValidate.Data.UserId);

            if (entityUser == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка зміни паролю!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            if (resultValidate.Data.NewPasswordHash is not string newHash)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Дані запиту пошкоджені!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            entityUser.PasswordHash = resultValidate.Data.NewPasswordHash;
            entityUser.UpdatedAt = DateTime.UtcNow;
            resultValidate.Data.IsConfirmed = true;

            await _userPendingAccountChangesRepository.UpdateAsync(resultValidate.Data);
            await _userRepository.UpdateAsync(entityUser);

            return new Result()
            {
                IsSuccess = true,
                Message = "Пароль змінено!",
                StatusCode = StatusCode.General.Ok
            };
        }

        public async Task<Result<ResetPasswordResponse>> ResetPasswordAsync(string login)
        {
            var typeLogin = _contactTypeDetector.GetLoginType(login);

            var loginType = typeLogin.IsSuccess ? typeLogin.Data : LoginType.Email;

            var step = loginType switch
            {
                LoginType.Email => Enum.ResetPasswordStep.EmailSend,
                LoginType.Phone => Enum.ResetPasswordStep.SmsCodeRequired,
                _ => Enum.ResetPasswordStep.EmailSend
            };

            var channel = loginType switch
            {
                LoginType.Email => CommunicationChannel.Email,
                LoginType.Phone => CommunicationChannel.Phone,
                _ => CommunicationChannel.Email
            };

            var entityUser = await _userRepository.GetUserByLoginAsync(login);

            // Генеруємо фейковий Id користувача для імітації роботи методу ( захист від атак підбору логінів )
            var (createChangeEntity, token) = CreateEntity(entityUser?.Id ?? Guid.NewGuid(), UserChangeType.PasswordReset);

            if(entityUser != null)
            {
                await _userPendingAccountChangesRepository.CreateAsync(createChangeEntity);

                var notificationEntity = new NotificationIntegrationDTO()
                {
                    UserId = entityUser!.Id,
                    Recipient = login,
                    Action = MessageActions.PasswordReset,
                    Channel = channel
                };

                var notificationResponse = await _notificationSender.SendCreateNotificationAsync(notificationEntity);

                if (!notificationResponse.IsSuccess)
                {
                    _logger.LogCritical("Сервіс сповіщення не відповідає");
                }
            }
            else
            {
                // Імітація затримки відправки
                _ = _hasher.Hash(token);
                await Task.Delay(Random.Shared.Next(120, 300));
            }

            return new Result<ResetPasswordResponse>()
            {
                IsSuccess = true,
                Message = "Якщо обліковий запис існує, інструкції було надіслано",
                StatusCode = StatusCode.General.Ok,
                Data = new ResetPasswordResponse()
                {
                    ResetPasswordStep = step,
                    ConfirmationToken = token,
                    RequestId = createChangeEntity.Id
                }
            };
        }

        public async Task<Result<string>> VerifyResetCodeAsync(VerifyResetDTO verifyResetDTO)
        {
            if (string.IsNullOrEmpty(verifyResetDTO.Code))
            {
                return new Result<string>()
                {
                    IsSuccess = false,
                    Message = "Недійсний код",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var resultChang = await _userPendingAccountChangesRepository.GetPendingChangesAsync(verifyResetDTO.RequestId);

            if (resultChang == null)
            {
                return new Result<string>()
                {
                    IsSuccess = false,
                    Message = "Помилка верифікації користувача!",
                    StatusCode = StatusCode.General.NotFound
                };
            }

            var code = await _verificationSender.GetCodeAsync(verifyResetDTO.RequestId);
            
            if (!code.IsSuccess || code.Data == null)
            {
                return new Result<string>()
                {
                    IsSuccess = false,
                    Message = "Помилка сервісу верифікації!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            if (!_hasher.Verify(code.Data, verifyResetDTO.Code))
            {
                return new Result<string>()
                {
                    IsSuccess = false,
                    Message = "Недійсний код",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            await _verificationSender.UpdateAsync(resultChang.Id, true);

            var temporaryToken = _tokenService.CreateTemporaryToken(resultChang.Id);

            return new Result<string>()
            {
                IsSuccess = true,
                Message = "Код вірний!",
                StatusCode = StatusCode.General.Ok,
                Data = temporaryToken
            };
        }

        public async Task<Result> FinalizePasswordResetAsync(FinalizePasswordResetDTO finalizePasswordResetDTO)
        {
            var requestId = _userContext.GetResetEventId();

            if (requestId == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка отримання ідентифікатора користувача!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var changeEntity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(requestId.Value);

            if (changeEntity == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка отримання ідентифікатора користувача!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var userEntity = await _userRepository.GetUserByIdAsync(changeEntity.UserId);

            if(userEntity == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка ідентифікатора користувача!",
                    StatusCode = StatusCode.General.NotFound
                };
            }

            var passwordHash = _hasher.Hash(finalizePasswordResetDTO.NewPassword);

            userEntity.PasswordHash = passwordHash;
            changeEntity.IsConfirmed = true;

            await _userRepository.UpdateAsync(userEntity);

            await _userPendingAccountChangesRepository.UpdateAsync(changeEntity);

            return new Result()
            {
                IsSuccess = true,
                Message = "Пароль оновлений!",
                StatusCode = StatusCode.General.Ok
            };
        }

        private (UserPendingChange entity, string token) CreateEntity(Guid userId, UserChangeType userChangeType, string? newPassword = null, string? newLogin = null)
        {
            string confirmationToken = GenerateToken();

            string confirmationTokenHash = _hasher.Hash(confirmationToken);

            //string passwordHash = _hasher.Hash(newPassword);

            var entity =  new UserPendingChange()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConfirmationToken = confirmationTokenHash,
                UserChangeType = userChangeType,
                NewPasswordHash = newPassword != null ? _hasher.Hash(newPassword) : null,
                NewLogin = newLogin,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsConfirmed = false
            };

            return (entity, confirmationToken);
        }

        private string GenerateToken()
        {
            var randomNumber = new byte[32];

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}
