using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Context;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.ExternalServices;
using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Exceptions;
using Contracts.Enum;
using Contracts.Integration;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        
        private readonly ILogger<AuthorizationService> _logger;

        private readonly IUserRepository _userRepository;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IRegistrationOrchestrator _orchestrator;
        private readonly IContactTypeDetector _contactTypeDetector;

        public AuthorizationService(
            IUserRepository userRepository,
            ILogger<AuthorizationService> logger,
            IPasswordHasher passwordHasher,
            IRegistrationOrchestrator orchestrator,
            IContactTypeDetector contactTypeDetector)
        {
            _userRepository = userRepository;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _orchestrator = orchestrator;
            _contactTypeDetector = contactTypeDetector;
        }

        public Task<UserResponseDTO> LoginAsync(UserLoginDTO userLoginDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDTO> RegistrationAsync(UserRegistrationDTO userRegistrationDTO)
        {
            _logger.LogInformation("Початок реєстрації користувача!");

            // 1. Перевірка на існування користувача
            var existingUser = await _userRepository.FindUserByLoginAsync(userRegistrationDTO.Login);
            
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(existingUser.Login);
            }

            // 2. Створення сутності
            var newUser = CreateNewUserEntity(userRegistrationDTO);

            // 3. Збереження в базу
            var creationResult = await _userRepository.CreateAsync(newUser);

            // 4. HTTP виклики до сервісів
            var context = new RegistrationContext()
            {
                UserId = newUser.Id,
                Login = newUser.Login,
                Channel = newUser.LoginType switch
                {
                    LoginType.Phone => CommunicationChannel.Phone,
                    LoginType.Email => CommunicationChannel.Email,
                    _ => throw new ArgumentOutOfRangeException(nameof(newUser.LoginType), "Unknown login type")
                },
                Action = MessageActions.Registration
            };

            var result = await _orchestrator.RunIntegrationsAsync(context);

            if (!result.IsSuccess)
            {
                await _userRepository.DeleteAsync(newUser.Id);

                // Додати правильну перевірку , і правильно повертати тип
                return new UserResponseDTO()
                {
                    Login = null
                };
            }
        }

        public Task<bool> LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserResponseDTO> RefreshSessionAsync()
        {
            throw new NotImplementedException();
        }

        private User CreateNewUserEntity(UserRegistrationDTO userRegister)
        {
            string encryptionPassword = _passwordHasher.HashPassword(userRegister.Password);

            LoginType typeLogin = _contactTypeDetector.GetLoginType(userRegister.Login);

            return new User()
            {
                Id = Guid.NewGuid(),
                Login = userRegister.Login,
                LoginType = typeLogin,
                PasswordHash = encryptionPassword,
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                LastLoginAt = null,
                IsConfirmed = false,
                IsBlocked = false,
                IsDeleted = false,
            };
        }
    }
}
