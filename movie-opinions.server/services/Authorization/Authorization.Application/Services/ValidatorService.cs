using Authorization.Application.DTO.Validator;
using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Contracts.Models.Status;

namespace Authorization.Application.Services
{
    public class ValidatorService : IValidatorService
    {
        private readonly IUserPendingAccountChangesRepository _userPendingAccountChangesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContext _userContext;
        private readonly IHasher _hasher;

        public ValidatorService(IUserPendingAccountChangesRepository userPendingAccountChangesRepository,
           IUserRepository userRepository,
           IUserContext userContext,
           IHasher hasher)
        {
            _userPendingAccountChangesRepository = userPendingAccountChangesRepository;
            _userRepository = userRepository;
            _userContext = userContext;
            _hasher = hasher;
        }

        public async Task<ValidationResult<UserPendingChange>> ValidateForConfirm(Guid requestId, string confirmationToken)
        {
            var authorizedUserId = GetAuthorizedUserId();

            if (!authorizedUserId.IsSuccess)
            {
                return new ValidationResult<UserPendingChange>()
                {
                    IsSuccess = false,
                    Message = "Користувач не в системі, будь-ласка авторизуйтесь знову!",
                    StatusCode = StatusCode.Auth.Unauthorized
                };
            }

            var entity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(requestId);

            var result = entity switch
            {
                null => (false, StatusCode.General.NotFound, "Запит не знайдено"),

                var e when e.UserId != authorizedUserId.Data
                    => (false, StatusCode.Auth.Unauthorized, "Помилка ідентифікації користувача"),

                var e when e.IsConfirmed => (false, StatusCode.General.BadRequest, "Запит уже виконаний"),

                var e when !_hasher.Verify(confirmationToken, e.ConfirmationToken)
                    => (false, StatusCode.Auth.Unauthorized, "Недійсний токен"),

                var e when e.ExpiresAt < DateTime.UtcNow
                    => (false, StatusCode.General.BadRequest, "Час вичерпано"),

                _ => (true, StatusCode.General.Ok, string.Empty)
            };

            return new ValidationResult<UserPendingChange>()
            {
                IsSuccess = result.Item1,
                Message = result.Item3,
                StatusCode = result.Item2,
                Data = result.Item1 ? entity : null
            };
        }

        public async Task<ValidationResult<Guid>> ValidateForSend(Guid requestId, string confirmationToken)
        {
            var authorizedUserId = GetAuthorizedUserId();

            if (!authorizedUserId.IsSuccess)
            {
                return authorizedUserId;
            }

            var entity = await _userPendingAccountChangesRepository.GetPendingChangesAsync(requestId);

            var result = entity switch
            {
                null => (false, StatusCode.General.NotFound, "Запит не знайдено"),

                var e when e.UserId != authorizedUserId.Data
                    => (false, StatusCode.Auth.Unauthorized, "Помилка ідентифікації користувача"),

                var e when e.IsConfirmed => (false, StatusCode.General.BadRequest, "Запит уже виконаний"),

                var e when !_hasher.Verify(confirmationToken, e.ConfirmationToken)
                    => (false, StatusCode.Auth.Unauthorized, "Недійсний токен"),

                var e when e.ExpiresAt < DateTime.UtcNow
                    => (false, StatusCode.General.BadRequest, "Час вичерпано"),

                _ => (true, StatusCode.General.Ok, string.Empty)
            };

            return new ValidationResult<Guid>()
            {
                IsSuccess = result.Item1,
                Message = result.Item3,
                StatusCode = result.Item2,
                Data = authorizedUserId.Data
            };
        }

        public async Task<ValidationResult<Guid>> ValidateForUser(string oldPassword, string newPassword)
        {
            var authorizedUserId = GetAuthorizedUserId();

            if (!authorizedUserId.IsSuccess)
            {
                return authorizedUserId;
            }

            var userEntity = await _userRepository.GetUserByIdAsync(authorizedUserId.Data);

            var result = userEntity switch
            {
                null => (false, StatusCode.General.NotFound, "Користувача не знайдено!"),

                var e when e.IsBlocked => (false, StatusCode.Auth.Locked, "Користувач заблокований!"),

                var e when !_hasher.Verify(oldPassword, userEntity.PasswordHash)
                    => (false, StatusCode.General.BadRequest, "Невірний пароль!"),

                var e when _hasher.Verify(newPassword, userEntity.PasswordHash)
                    => (false, StatusCode.General.BadRequest, "Пароль не може бути тим самим!"),

                _ => (true, StatusCode.General.Ok, string.Empty)
            };

            return  new ValidationResult<Guid> ()
            {
                IsSuccess = result.Item1,
                Message = result.Item3,
                StatusCode = result.Item2,
                Data = authorizedUserId.Data
            };
        }

        private ValidationResult<Guid> GetAuthorizedUserId()
        {
            var userId = _userContext.GetUserId();

            if (userId == null)
            {
                return new ValidationResult<Guid>()
                {
                    IsSuccess = false,
                    Message = "Користувач не в системі, будь-ласка авторизуйтесь знову!",
                    StatusCode = StatusCode.Auth.Unauthorized
                };
            }

            return new ValidationResult<Guid>()
            {
                IsSuccess = true,
                Message = "Користувача ідентифіковано!",
                StatusCode = StatusCode.General.Ok,
                Data = userId.Value
            };
        }
    }
}
