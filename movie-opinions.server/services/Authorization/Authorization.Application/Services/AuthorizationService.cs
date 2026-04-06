using Authorization.Application.Common;
using Authorization.Application.DTO.Authentication.Commands;
using Authorization.Application.DTO.Authentication.Results;
using Authorization.Application.Enum;
using Authorization.Application.Interfaces.Identity;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;

        private readonly IUserRepository _userRepository;
        private readonly IUserPendingRegistrationRepository _userPendingRegistrationRepository;

        private readonly IHasher _hasher;
        private readonly IUserContext _userContext;

        public AuthorizationService(
            ILogger<AuthorizationService> logger,
            IUserRepository userRepository,
            IHasher hasher,
            IUserContext userContext,
            IUserPendingRegistrationRepository userPendingRegistrationRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _hasher = hasher;
            _userContext = userContext;
            _userPendingRegistrationRepository = userPendingRegistrationRepository;
        }

        public Task<Result> LoginAsunc(LoginCommand loginCommand)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<RegistrationResult>> RegistrationAsync(RegistrationCommand registrationCommand)
        {
            _logger.LogInformation("Початок реєстрації користувача {Login}!", registrationCommand.Login);

            // 1. Ініціалізація доменного об'єкта логіна (валідація формату)
            var loginVo = Login.Create(registrationCommand.Login);

            // 2. Валідація унікальності: перевірка, чи не зайнятий логін активним акаунтом
            var isUserExists = await _userRepository.ExistsByLoginAsync(loginVo);

            if(isUserExists)
            {
                return Result<RegistrationResult>.Failure(ErrorCode.UserAlreadyExists, "Спробуйте будь-ласка інший логін!");
            }

            // 3. Криптографічне хешування пароля та створення Value Object
            var passwordHash = new Password(_hasher.Hash(registrationCommand.Password));

            // 4. Пошук існуючої сесії очікування реєстрації (Staging area)
            var existingRegistration = await _userPendingRegistrationRepository.GetByLoginAsync(loginVo);

            if (existingRegistration != null)
            {
                // Оновлення існуючого запису (пролонгація терміну дії та зміна пароля)
                existingRegistration.Refresh(passwordHash);

                await _userPendingRegistrationRepository.UpdateAsync(existingRegistration);
            }
            else
            {
                // 5. Створення запису ініціації реєстрування
                var newRegistration = UserPendingRegistration.Create(loginVo, passwordHash);

                await _userPendingRegistrationRepository.CreateAsync(newRegistration);
            }

            // 6. Визначення стратегії підтвердження на основі типу логіна
            var nextSpet = loginVo.Type switch
            {
                LoginType.Login_Email => RegistrationStep.EmailConfirmation,
                LoginType.Login_Phone => RegistrationStep.SmsConfirmation,
                _ => throw new InvalidOperationException("Невідомий тип логіна")
            };

            // 7. HTTP виклики до сервісів


            return Result<RegistrationResult>.Success(new RegistrationResult()
            {
                RegistrationStep = nextSpet
            });
        }

        public Task<Result> LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result> RefreshSessionAsync()
        {
            throw new NotImplementedException();
        }
    }
}
