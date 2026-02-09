using Authorization.Application.Interfaces.Integration;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.DAL.Interface;
using Authorization.Domain.DTO;
using Authorization.Domain.Entities;
using Authorization.Domain.Models;
using Authorization.Domain.Request;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IAccessService _accessService;
        private readonly ITokenService _tokenService;
        

        public AuthorizationService(
            ILogger<AuthorizationService> logger, 
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher,
            ISendInternalRequest sendInternalRequest,
            IAccessService accessService,
            ITokenService tokenService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _sendInternalRequest = sendInternalRequest;
            _accessService = accessService;
            _tokenService = tokenService;
        }

        public async Task<ServiceResponse<UserResponseDTO>> LoginAsync(UserLoginModel loginModel)
        {
            _logger.LogInformation("Початок роботи логування");

            try
            {
                // 1. Перевірка на існування користувача
                var existingUser = await _userRepository.GetUserByEmailAsync(loginModel.Email);

                if(existingUser.StatusCode != StatusCode.General.Ok)
                {
                    _logger.LogWarning("Користувача {Email} не знайдено, або виникла помилка. Код помилки {StatusCode}", loginModel.Email, existingUser.StatusCode);

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = existingUser.StatusCode,
                        Message = existingUser.Message,
                    };
                }

                // 2. Перевірка паролю
                bool isPasswordCorrect = await _passwordHasher.VerifyPasswordAsync(
                    loginModel.Password,
                    existingUser.Data.PasswordSalt,
                    existingUser.Data.PasswordHash);

                if(!isPasswordCorrect)
                {
                    _logger.LogInformation("Невірний логін або пароль!");

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.Auth.Unauthorized,
                        Message = "Невірний логін або пароль!"
                    };
                }

                var isAcces = await _accessService.CheckUserAccess(existingUser.Data);

                if (!isAcces.IsSuccess)
                {
                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = isAcces.IsSuccess,
                        StatusCode = isAcces.StatusCode,
                        Message = isAcces.Message,
                    };
                }

                var userIdentity = new UserSessionIdentity()
                {
                    UserId = existingUser.Data.UserId,
                    Email = existingUser.Data.Email,
                    IsEmailConfirmed = existingUser.Data.IsEmailConfirmed,
                    Role = existingUser.Data.Role
                };

                return await _tokenService.CreateUserSessionAsync(userIdentity);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Сталась помилка серверу!");

                return new ServiceResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка серверу!"
                };
            }
        }

        public async Task<ServiceResponse<UserResponseDTO>> RegisterAsync(UserRegisterModel registerModel)
        {
            _logger.LogInformation("Початок роботи сервісу реєстрації.");

            try
            {
                // 1. Перевірка на існування користувача
                var existingUser = await _userRepository.GetUserByEmailAsync(registerModel.Email);

                if(existingUser.StatusCode != StatusCode.General.NotFound)
                {
                    _logger.LogWarning("Користувач з такою поштою {Email} не можу зареєструватися. Код помилки {StatusCode}", registerModel.Email, existingUser.StatusCode);

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = existingUser.StatusCode,
                        Message = existingUser.Message,
                    };
                }

                // 2. Створення сутності
                var newUser = await CreateNewUserEntityAsync(registerModel);

                // 3. Збереження в базу
                _logger.LogInformation("Збереження користувача в базу!");

                var creationResult = await _userRepository.CreateAsync(newUser);

                if(creationResult.StatusCode != StatusCode.Create.Created)
                {
                    _logger.LogWarning("Помилка при збереженні користувача в базі даних. Код помилки {StatusCode}. Текст помилки : {Message}",
                        creationResult.StatusCode, creationResult.Message);

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        Message = creationResult.Message,
                        StatusCode= creationResult.StatusCode
                    };
                }

                // 4. HTTP виклик до ProfileService
                _logger.LogInformation("Виклик сервісу профілів!");

                var profileRequest = new InternalRequest<ProfileCreateIntegrationDTO>
                {
                    ClientName = "ProfileClient",
                    Endpoint = "api/profile/create",
                    Method = HttpMethod.Post,
                    Body = new ProfileCreateIntegrationDTO()
                    {
                        UserId = creationResult.Data.UserId,
                        Email = creationResult.Data.Email
                    }
                };

                try
                {
                    var responseProfile = await _sendInternalRequest.SendAsync<ProfileCreateIntegrationDTO, Guid>(profileRequest);
                
                    if (!responseProfile.IsSuccess)
                    {
                        // ROLLBACK: якщо профіль не створився — видаляємо юзера з Auth
                        await RollbackUserRegistrationAsync(newUser.UserId, responseProfile.Message);
                
                        _logger.LogError("Помилка сервісу профілів. Код помилки {StatusCOde}", responseProfile.StatusCode);
                
                        return new ServiceResponse<UserResponseDTO>()
                        {
                            IsSuccess = false,
                            StatusCode = responseProfile.StatusCode,
                            Message = responseProfile?.Message ?? "Сервіс профілів повернув помилку"
                        };
                    }
                }
                catch (Exception ex)
                {
                    // ROLLBACK: якщо профіль не створився — видаляємо юзера з Auth
                    await RollbackUserRegistrationAsync(newUser.UserId, ex.Message);
                
                    _logger.LogError("Помилка сервісу профілів. Текст помилки: {ex}", ex.Message);
                
                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.General.InternalError,
                        Message = "Помилка зв'язку з сервісом профілів.",
                    };
                }

                // 5. HTTP виклик до NotificationService
                var notificationRequest = new InternalRequest<NotificationCreateIntegrationDTO>
                {
                    ClientName = "NotificationClient",
                    Endpoint = "api/notification/send",
                    Method = HttpMethod.Post,
                    Body = new NotificationCreateIntegrationDTO()
                    {
                        IdUser = newUser.UserId,
                        Recipient = newUser.Email,
                        Channel = "Email",
                        TemplateName = "Registration",
                        TemplateData = new Dictionary<string, string>
                            {
                                { "UserName", newUser.Email },
                                { "AppName", "Movie Opinions" }
                            }
                    }
                };

                bool isNotificationSent = false;

                try
                {
                    var responseNotification = await _sendInternalRequest.SendAsync<NotificationCreateIntegrationDTO, object>(notificationRequest);
                
                    if (responseNotification.IsSuccess)
                    {
                        isNotificationSent = true;
                    }
                    else
                    {
                        _logger.LogWarning("NotificationService повернув помилку: {Msg}", responseNotification.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NotificationService недоступний (Offline).");
                }

                var userIdentity = new UserSessionIdentity()
                {
                    UserId = newUser.UserId,
                    Email = newUser.Email,
                    IsEmailConfirmed = newUser.IsEmailConfirmed,
                    Role = newUser.Role
                };

                var sessionResult = await _tokenService.CreateUserSessionAsync(userIdentity);

                if (!sessionResult.IsSuccess)
                {
                    _logger.LogError("Помилка реєстрації!");

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = sessionResult.StatusCode,
                        Message = sessionResult.Message
                    };
                }

                return new ServiceResponse<UserResponseDTO>()
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Data = sessionResult.Data,
                    Message = isNotificationSent
                        ? "Реєстрація успішна! Перевірте вашу пошту для підтвердження."
                        : "Реєстрація успішна! Але виникла проблема з відправкою листа. Надішліть його повторно в налаштуваннях."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критична помилка сервісу!");

                return new ServiceResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка!"
                };
            }
        }

        public async Task<ServiceResponse<bool>> LogoutAsync()
        {
            try
            {
                var clearToken =  await _tokenService.ClearCookies();

                if(clearToken.IsSuccess != true)
                {
                    return new ServiceResponse<bool>()
                    {
                        IsSuccess = false,
                        StatusCode = clearToken.StatusCode,
                        Message = clearToken.Message,
                        Data = false
                    };
                }

                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Message = "Вихід успішний!",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при виході!");

                return new ServiceResponse<bool>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка системи при спробі виходу!",
                    Data = false
                };
            }
        }

        public async Task<ServiceResponse<UserResponseDTO>> RefreshSessionAsync()
        {
            try
            {
                var refreshToken = await _tokenService.ValidateAndRevokeTokenAsync();

                if(refreshToken.StatusCode != StatusCode.General.Ok)
                {
                    _logger.LogWarning("Помилка валідації токену: {Message}", refreshToken.Message);

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = refreshToken.StatusCode,
                        Message = refreshToken.Message,
                    };
                }

                var informationUser = await _userRepository.GetUserByIdAsync(refreshToken.Data);

                if(informationUser.StatusCode != StatusCode.General.Ok)
                {
                    _logger.LogInformation("Помилка читання користувача із бази");

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = informationUser.StatusCode,
                        Message = informationUser.Message,
                    };
                }

                var userIdentity = new UserSessionIdentity()
                {
                    UserId = informationUser.Data.UserId,
                    Email = informationUser.Data.Email,
                    IsEmailConfirmed = informationUser.Data.IsEmailConfirmed,
                    Role = informationUser.Data.Role
                };

                var sessionResult = await _tokenService.CreateUserSessionAsync(userIdentity);

                if (!sessionResult.IsSuccess)
                {
                    _logger.LogError("Помилка отримання нового токену!");

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = sessionResult.StatusCode,
                        Message = sessionResult.Message
                    };
                }

                return new ServiceResponse<UserResponseDTO>()
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Data = sessionResult.Data,
                    Message = "Токени успішно оновлений!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при спробі оновити сесію користувача!");

                return new ServiceResponse<UserResponseDTO>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Помилка системи при спробі оновити токен користувача!"
                };
            }
        }

        private async Task<User> CreateNewUserEntityAsync(UserRegisterModel userRegisterModel)
        {
            _logger.LogInformation("Створення сутності користувача");

            string passwordSalt = Guid.NewGuid().ToString();
            string encryptionPassword = await _passwordHasher.HashPasswordAsync(userRegisterModel.Password, passwordSalt);

            return new User()
            {
                UserId = Guid.NewGuid(),
                Email = userRegisterModel.Email,
                PasswordHash = encryptionPassword,
                PasswordSalt = passwordSalt,
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                IsBlocked = false,
                IsDeleted = false,
            };
        }

        private async Task RollbackUserRegistrationAsync(Guid idUser, string originalError)
        {
            _logger.LogWarning("Запуск відкату: Видалення користувача {UserId} через помилку: {Reason}", idUser, originalError);

            var deleteResult = await _userRepository.DeleteAsync(idUser);

            if (!deleteResult.IsSuccess)
            {
                _logger.LogCritical("КРИТИЧНО: Не вдалося видалити юзера {UserId} під час відкату! Статус: {Status}. Помилка: {Msg}",
                    idUser, deleteResult.StatusCode, deleteResult.Message);
            }
            else
            {
                _logger.LogInformation("Відкат завершено. Користувач {Email} (ID: {Id}) видалений.",
                    deleteResult.Data?.Email, idUser);
            }
        }
    }
}