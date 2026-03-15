using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Context;
using Authorization.Application.DTO.Users;
using Authorization.Application.DTO.Users.Response;
using Authorization.Application.Enum;
using Authorization.Application.Interfaces.Infrastructure;
using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Contracts.Enum;
using Contracts.Integration;
using Contracts.Models.Response;
using Contracts.Models.Status;
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

        private readonly ITokenService _tokenService;
        private readonly IAccessService _accessService;

        public AuthorizationService(
            IUserRepository userRepository,
            ILogger<AuthorizationService> logger,
            IPasswordHasher passwordHasher,
            IRegistrationOrchestrator orchestrator,
            IContactTypeDetector contactTypeDetector,
            ITokenService tokenService,
            IAccessService accessService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _orchestrator = orchestrator;
            _contactTypeDetector = contactTypeDetector;
            _tokenService = tokenService;
            _accessService = accessService;
        }

        public async Task<Result<LoginResponseDTO>> LoginAsync(UserLoginDTO userLoginDTO)
        {
            _logger.LogInformation("Вхід користувача {Login}", userLoginDTO.Login);

            // 1. Перевірка на існування користувача та перевірка паролю
            var existingUser = await _userRepository.GetUserByLoginAsync(userLoginDTO.Login);
            
            if(existingUser == null || !_passwordHasher.VerifyPassword(userLoginDTO.Password, existingUser.PasswordHash))
            {
                return new Result<LoginResponseDTO>()
                {
                    IsSuccess = false,
                    Message = "Невірний логін або пароль",
                    StatusCode = StatusCode.Auth.Unauthorized,
                    Data = new LoginResponseDTO()
                    {
                        Role = Role.Guest
                    }
                };
            }

            // 2. Первірка доступу (якщо має блокування або видалення)
            if(existingUser.IsBlocked || existingUser.IsDeleted)
            {
                var accesUser = new UserAccessDTO()
                {
                    UserId = existingUser.Id,
                    IsBlocked = existingUser.IsBlocked,
                    IsDeleted = existingUser.IsDeleted,
                };

                var isAcces = await _accessService.CheckUserAccess(accesUser);

                if (isAcces.PropertiesToReset.Any())
                {
                    var userType = existingUser.GetType();

                    foreach (var propertyName in isAcces.PropertiesToReset)
                    {
                        var prop = userType.GetProperty(propertyName);

                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(existingUser, false);
                        }
                    }

                    await _userRepository.UpdateAsync(existingUser);
                }

                if (!isAcces.IsAllowed)
                {
                    return new Result<LoginResponseDTO>()
                    {
                        IsSuccess = false,
                        Message = isAcces.Message,
                        StatusCode = isAcces.StatusCode,
                        Data = new LoginResponseDTO()
                        {
                            Login = existingUser.Login,
                            Role = Role.Guest
                        }
                    };
                }
            }

            // 3. Створення токену
            var userSession = new UserSessionDTO()
            {
                UserId = existingUser.Id,
                Login = existingUser.Login,
                IsEmailConfirmed = existingUser.IsConfirmed,
                Role = existingUser.Role 
            };

            await _tokenService.CreateUserSessionAsync(userSession);

            // 4. Оновлення останнього входу користувача
            existingUser.LastLoginAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existingUser);

            return new Result<LoginResponseDTO>()
            {
                IsSuccess = true,
                Message = "Вхід успішний",
                StatusCode = StatusCode.General.Ok,
                Data = new LoginResponseDTO()
                {
                    Login = existingUser.Login,
                    Role = existingUser.Role,
                }
            };
        }

        public async Task<Result<RegistrationResponseDTO>> RegistrationAsync(UserRegistrationDTO userRegistrationDTO)
        {
            _logger.LogInformation("Початок реєстрації користувача {Login}!", userRegistrationDTO.Login);

            // 1. Перевірка на існування користувача
            var existingUser = await _userRepository.GetUserByLoginAsync(userRegistrationDTO.Login);
            
            if (existingUser != null)
            {
                return new Result<RegistrationResponseDTO>()
                {
                    IsSuccess = false,
                    Message = "Спробуйте інший логін",
                    StatusCode = StatusCode.Create.Conflict
                };
            }

            // 2. Створення сутності
            var newUser = CreateNewUserEntity(userRegistrationDTO);

            // 3. Збереження в базу
            var creationResult = await _userRepository.CreateAsync(newUser);

            // 4. Створення токену
            var userSession = new UserSessionDTO()
            {
                UserId = newUser.Id,
                Login = newUser.Login,
                IsEmailConfirmed = false,
                Role = newUser.Role 
            };

            if(!await _tokenService.CreateUserSessionAsync(userSession))
            {
                _logger.LogError("Помилка при створенні токену для користувача {Login}", userSession.Login);
            
                await _userRepository.DeleteAsync(newUser.Id);
            
                return new Result<RegistrationResponseDTO>()
                {
                    IsSuccess = false,
                    Message = "Помилка при генерації токену. Спробуйте пізніше!",
                    StatusCode = StatusCode.General.InternalError
                };
            }

            // 5. HTTP виклики до сервісів
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
                await _tokenService.ClearAllUserSessionsAsync(newUser.Id);
            
                await _userRepository.DeleteAsync(newUser.Id);
            
                return new Result<RegistrationResponseDTO>()
                {
                    IsSuccess = false,
                    Message = result.Message,
                    StatusCode = StatusCode.General.InternalError
                };
            }

            return new Result<RegistrationResponseDTO>()
            {
                IsSuccess = true,
                Message = "Реєстрація успішна!",
                StatusCode = StatusCode.General.Ok,
                Data = new RegistrationResponseDTO()
                {
                    Login = newUser.Login,
                    RegistrationStep = newUser.LoginType switch
                    {
                        LoginType.Phone => RegistrationStep.SmsCodeRequired,
                        LoginType.Email => RegistrationStep.EmailConfirmationSent,
                        _ => throw new ArgumentOutOfRangeException(nameof(newUser.LoginType), "Unknown login type")
                    },
                    Role = newUser.Role
                }
            };
        }

        public async Task<Result<LogoutResponseDTO>> LogoutAsync()
        {
            var clearSession =  await _tokenService.DeleteSessionAsync();

            return new Result<LogoutResponseDTO>()
            {
                IsSuccess = true,
                Message = "Вихід успішний!",
                StatusCode = StatusCode.General.Ok,
                Data = new LogoutResponseDTO()
                {
                    Role = Role.Guest
                }
            };
        }

        public async Task<Result<LoginResponseDTO>> RefreshSessionAsync()
        {
            var refreshToken = await _tokenService.ValidateRefreshTokenAsync();

            if(refreshToken == null)
            {
                await _tokenService.DeleteSessionAsync();

                return new Result<LoginResponseDTO>()
                {
                    IsSuccess = false,
                    Message = "Сесія скінчилась!",
                    StatusCode = StatusCode.Verification.Expired,
                    Data = new LoginResponseDTO()
                    {
                        Role = Role.Guest
                    }
                };
            }

            var entityUser = await _userRepository.GetUserByIdAsync(refreshToken.UserId);

            if(entityUser == null || entityUser.IsDeleted || entityUser.IsBlocked)
            {
                await _tokenService.DeleteSessionAsync();

                return new Result<LoginResponseDTO>()
                {
                    IsSuccess = false,
                    Message = "Сесія скінчилась!",
                    StatusCode = StatusCode.Verification.Expired,
                    Data = new LoginResponseDTO()
                    {
                        Role = Role.Guest
                    }
                };
            }

            await _tokenService.DeleteSessionAsync();

            var userSession = new UserSessionDTO()
            {
                UserId = entityUser.Id,
                Login = entityUser.Login,
                IsEmailConfirmed = entityUser.IsConfirmed,
                Role = entityUser.Role
            };

            await _tokenService.CreateUserSessionAsync(userSession);

            return new Result<LoginResponseDTO>()
            {
                IsSuccess = true,
                Message = "Сесія оновлена!",
                StatusCode = StatusCode.General.Ok,
                Data = new LoginResponseDTO()
                {
                    Login = userSession.Login,
                    Role = entityUser.Role,
                }
            };
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
