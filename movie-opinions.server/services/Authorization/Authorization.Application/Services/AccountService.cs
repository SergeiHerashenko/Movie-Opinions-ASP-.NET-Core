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

namespace Authorization.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserPendingAccountChangesRepository _userPendingAccountChangesRepository;
        private readonly IContactsSender _contactsSender;
        private readonly INotificationSender _notificationSender;
        private readonly IVerificationSender _verificationSender;

        public AccountService(IUserContext userContext,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUserPendingAccountChangesRepository userPendingAccountChangesRepository,
            IContactsSender contactsSender,
            INotificationSender notificationSender,
            IVerificationSender verificationSender)
        {
            _userContext = userContext;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _userPendingAccountChangesRepository = userPendingAccountChangesRepository;
            _contactsSender = contactsSender;
            _notificationSender = notificationSender;
            _verificationSender = verificationSender;
        }

        public async Task<Result<InitiatePasswordChangeResponse>> InitiatePasswordChangeAsync(InitiatePasswordChangeDTO initiatePasswordChangeDTO)
        {
            string? loginUser = _userContext.GetUserLogin();

            if(string.IsNullOrEmpty(loginUser))
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "Помилка отримання логіну користувача!",
                    StatusCode = StatusCode.Auth.Unauthorized
                };
            }

            var userEntity = await _userRepository.GetUserByLoginAsync(loginUser);

            if (userEntity == null)
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "Помилка отримання логіну користувача!",
                    StatusCode = StatusCode.General.NotFound
                };
            }

            if (userEntity.IsBlocked)
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "КОристувач заблокований!",
                    StatusCode = StatusCode.Auth.Locked
                };
            }

            if(!_passwordHasher.VerifyPassword(initiatePasswordChangeDTO.OldPassword, userEntity.PasswordHash))
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "Невірний пароль!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var contactsResponse = await _contactsSender.GetUserChannelsAsync(userEntity.Id);

            if (!contactsResponse.IsSuccess)
            {
                return new Result<InitiatePasswordChangeResponse>()
                {
                    IsSuccess = false,
                    Message = "Помилка при отриманні списку контактів!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var createChangeEntity = CreateEntity(initiatePasswordChangeDTO.NewPassword, userEntity.Id);

            var saveChangeEntity = await _userPendingAccountChangesRepository.CreateAsync(createChangeEntity);

            return new Result<InitiatePasswordChangeResponse>()
            {
                IsSuccess = true,
                Message = "Пароль вірний",
                StatusCode = StatusCode.General.Ok,
                Data = new InitiatePasswordChangeResponse()
                {
                    RequestId = saveChangeEntity.Id,
                    CommunicationChannel = contactsResponse.Data!.Select(c => new ContactResponseDTO
                    {
                        UserId = c.UserId,
                        ContactValue = MaskContactValue(c.ContactValue, c.CommunicationChannel),
                        CommunicationChannel = c.CommunicationChannel
                    }).ToList()
                }
            };
        }

        public async Task<Result> SendVerificationCodeAsync(SendVerificationCodeDTO sendVerificationCodeDTO)
        {
            var entity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(sendVerificationCodeDTO.RequestId);

            if(entity == null || entity.ExpiresAt < DateTime.UtcNow)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Час зміни вийшов!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var contactsResponse = await _contactsSender.GetUserChannelsAsync(entity.UserId);

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
                UserId = entity.UserId,
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

            var entity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(passwordConfirmationDTO.RequestId);

            if (entity == null || entity.ExpiresAt < DateTime.UtcNow)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Час зміни вийшов!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var code = await _verificationSender.GetCode(entity.UserId);

            if (!code.IsSuccess || code.Data == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка сервісу верифікації!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            if (!_passwordHasher.VerifyPassword(code.Data, passwordConfirmationDTO.Code))
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Недійсний код",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            var entityUser = await _userRepository.GetUserByIdAsync(entity.UserId);

            if (entityUser == null)
            {
                return new Result()
                {
                    IsSuccess = false,
                    Message = "Помилка зміни паролю!",
                    StatusCode = StatusCode.General.BadRequest
                };
            }

            entityUser.PasswordHash = entity.NewPasswordHash!;
            entityUser.UpdatedAt = DateTime.UtcNow;
            entity.IsConfirmed = true;

            await _userPendingAccountChangesRepository.UpdateAsync(entity);
            await _userRepository.UpdateAsync(entityUser);

            return new Result()
            {
                IsSuccess = true,
                Message = "Пароль змінено!",
                StatusCode = StatusCode.General.Ok
            };
        }

        private UserPendingChange CreateEntity(string newPassword, Guid userId)
        {
            string confirmationToken = GenerateToken();

            string passwordHash = _passwordHasher.HashPassword(newPassword);

            return new UserPendingChange()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ConfirmationToken = confirmationToken,
                UserChangeType = UserChangeType.PasswordChange,
                NewPasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
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

        private string MaskContactValue(string value, CommunicationChannel channel)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (channel == CommunicationChannel.Email)
            {
                var parts = value.Split('@');
                if (parts.Length < 2) return value;

                string name = parts[0];
                string domain = parts[1];

                if (name.Length <= 2) return $"{name[0]}***@{domain}";

                return $"{name.Substring(0, 2)}****{name.Substring(name.Length - 1)}@{domain}";
            }
            else if (channel == CommunicationChannel.Phone)
            {
                if (value.Length < 4) return value;
                return $"{value.Substring(0, value.Length - 4)}****{value.Substring(value.Length - 2)}";
            }

            return value;
        }
    }
}
