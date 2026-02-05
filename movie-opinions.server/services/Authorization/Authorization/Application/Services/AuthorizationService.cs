using Authorization.Application.Interfaces;
using Authorization.DAL.Interface;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Cookies.Interfaces;
using Authorization.Infrastructure.Cryptography;
using Authorization.Infrastructure.IdentityAccessor;
using Authorization.Infrastructure.InternalCommunication;
using Authorization.Infrastructure.JWT.Interfaces;
using Authorization.Models.User;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IIdentityAccessor _identityAccessor;
        private readonly IJwtProvider _jwtProvider;
        private readonly ICookieProvider _cookieProvider;
        private readonly IPasswordHasher _passwordHasher;

        public AuthorizationService(IAuthorizationRepository authorizationRepository,
            ICookieProvider cookieProvider,
            ISendInternalRequest sendInternalRequest,
            IIdentityAccessor identityAccessor,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher)
        {
            _authorizationRepository = authorizationRepository;
            _cookieProvider = cookieProvider;
            _sendInternalRequest = sendInternalRequest;
            _identityAccessor = identityAccessor;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResponse<AuthorizationUserDTO>> LoginAsync(UserLoginModel loginModel)
        {
            try
            {
                var getUser = await _authorizationRepository.GetUserByEmailAsync(loginModel.Email);

                if (getUser.StatusCode != StatusCode.General.Ok)
                {
                    return new ServiceResponse<AuthorizationUserDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = getUser.StatusCode,
                        Message = getUser.Message,
                    };
                }

                bool isPasswordCorrect = await _passwordHasher.VerifyPasswordAsync(
                    loginModel.Password,
                    getUser.Data.PasswordSalt,
                    getUser.Data.PasswordHash);

                if (!isPasswordCorrect)
                {
                    return new ServiceResponse<AuthorizationUserDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = StatusCode.Auth.Unauthorized,
                        Message = "Невірний логін або пароль!"
                    };
                }

                var isAcces = CheckUserAccess(getUser.Data);

                if(!isAcces.IsSuccess)
                {
                    return new ServiceResponse<AuthorizationUserDTO>()
                    {
                        IsSuccess = isAcces.IsSuccess,
                        StatusCode = isAcces.StatusCode,
                        Message = isAcces.Message,
                    };
                }

                return await CreateUserSessionAsync(getUser.Data);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ServiceResponse<AuthorizationUserDTO>> RegistrationAsync(UserRegisterModel registrationModel)
        {
            try
            {
                // 1. Перевірка на існування
                var getUser = await _authorizationRepository.GetUserByEmailAsync(registrationModel.Email);

                if (getUser.StatusCode != StatusCode.General.NotFound)
                {
                    return new ServiceResponse<AuthorizationUserDTO>
                    {
                        IsSuccess = false,
                        StatusCode = getUser.StatusCode,
                        Message = getUser.Message,
                    };
                }

                // 2. Створення сутності
                var newUser = await CreateNewUserEntityAsync(registrationModel);

                // 3. Збереження в Authorization базі
                var registerUser = await _authorizationRepository.CreateAsync(newUser);

                if (registerUser.StatusCode != StatusCode.Create.Created)
                {
                    return new ServiceResponse<AuthorizationUserDTO>
                    {
                        IsSuccess = false,
                        StatusCode = registerUser.StatusCode,
                        Message = registerUser.Message,
                    };
                }

                // 4. Створюємо опис запиту на ProfileService
                var profileRequest = new InternalRequest<object>
                {
                    ClientName = "ProfileClient",
                    Endpoint = "api/profile/create",
                    Method = HttpMethod.Post,
                    Body = new { newUser.UserId, newUser.Email }
                };

                try
                {
                    var responseProfile = await _sendInternalRequest.SendAsync<object, Guid>(profileRequest);

                    if (!responseProfile.IsSuccess)
                    {
                        // ROLLBACK: якщо профіль не створився — видаляємо юзера з Auth
                        await _authorizationRepository.DeleteAsync(newUser.UserId);

                        return new ServiceResponse<AuthorizationUserDTO>
                        {
                            IsSuccess = false,
                            StatusCode = (int)responseProfile.StatusCode,
                            Message = responseProfile?.Message ?? "Сервіс профілів повернув помилку"
                        };
                    }
                }
                catch (Exception ex)
                {
                    // ROLLBACK: якщо профіль не створився — видаляємо юзера з Auth
                    await _authorizationRepository.DeleteAsync(newUser.UserId);

                    return new ServiceResponse<AuthorizationUserDTO>
                    {
                        IsSuccess = false,
                        Message = "Помилка зв'язку з сервісом профілів." + ex.Message,
                    };
                }

                // 5. HTTP виклик до NotificationService
                var notificationRequest = new InternalRequest<object>
                {
                    ClientName = "NotificationClient",
                    Endpoint = "api/notification/send",
                    Method = HttpMethod.Post,
                    Body = new
                        {
                            IdUser = newUser.UserId,
                            Destination = newUser.Email,
                            Channel = "Email",
                            TemplateName = "Registration",
                            TemplateData = new Dictionary<string, string>
                            {
                                { "UserName", newUser.Email },
                                { "AppName", "Movie Opinions" }
                            }
                        }
                };

                var responseNotification = await _sendInternalRequest.SendAsync<object, object>(notificationRequest);

                var tokenModel = new UserTokenModel()
                {
                    UserId = registerUser.Data.UserId,
                    Email = registerUser.Data.Email,
                    IsEmailConfirmed = registerUser.Data.IsEmailConfirmed
                };

                var createUserSessionAsync = await CreateUserSessionAsync(newUser);

                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Data = createUserSessionAsync.Data,
                    Message = responseNotification.IsSuccess
                        ? "Реєстрація успішна! Перевірте вашу пошту для підтвердження."
                        : "Реєстрація успішна! Але виникла проблема з відправкою листа. Надішліть його повторно в налаштуваннях."       
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = "Критична помилка!" + ex.Message,
                };
            }
        }

        public async Task<ServiceResponse<bool>> LogoutAsync()
        {
            _cookieProvider.ClearAuthCookies();

            return new ServiceResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Вихід успішний",
                Data = true
            };
        }

        public async Task<ServiceResponse<AuthorizationUserDTO>> RefreshTokenAsync()
        {
            // 1. Отримуємо дані
            var userId = _identityAccessor.UserId;
            var refreshToken = _identityAccessor.RefreshToken;

            if (userId == null || string.IsNullOrEmpty(refreshToken))
                return new ServiceResponse<AuthorizationUserDTO> { IsSuccess = false, Message = "Сесія втрачена" };

            // 2. Шукаємо токен у базі
            var userToken = await _authorizationRepository.GetTokenAsync(refreshToken, userId.Value);

            // 3. Перевіряємо валідність
            if (userToken.Data == null || userToken.Data.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                return new ServiceResponse<AuthorizationUserDTO> { IsSuccess = false, Message = "Сесія прострочена" };
            }

            // 4. Шукаємо самого юзера, щоб створити йому нову сесію
            var user = await _authorizationRepository.GetUserByIdAsync(userId.Value);

            return await CreateUserSessionAsync(user.Data);
        }

        public async Task<ServiceResponse<AuthorizationUserDTO>> ChangePasswordAsync()
        {
            try
            {
                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = false,
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.InternalError,
                    Message = ex.Message,
                };
            }
        }

        private async Task<ServiceResponse<AuthorizationUserDTO>> CreateUserSessionAsync(User user)
        {
            // 1. Готуємо модель для JWT
            var tokenModel = new UserTokenModel()
            {
                UserId = user.UserId,
                Email = user.Email,
                IsEmailConfirmed = user.IsEmailConfirmed,
            };

            // 2. Генеруємо токени
            var accessToken = _jwtProvider.GenerateAccessToken(tokenModel);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            _cookieProvider.SetAuthCookies(accessToken, refreshToken);

            // 3. Формуємо сутність для БД
            var userToken = new UserTokenEntity()
            {
                IdToken = Guid.NewGuid(),
                IdUser = user.UserId,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            };

            // Додати видалення інших токенів користувача
            var saveResult = await _authorizationRepository.CreateTokenAsync(userToken);

            if (!saveResult.IsSuccess)
            {
                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = false,
                    StatusCode = saveResult.StatusCode,
                    Message = "Помилка при збереженні сесії."
                };
            }

            // 4. Повертаємо готовий DTO
            return new ServiceResponse<AuthorizationUserDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Data = new AuthorizationUserDTO
                {
                    UserId = user.UserId
                },
                Message = "Вхід успішний!"
            };
        }

        private ServiceResponse<AuthorizationUserDTO> CheckUserAccess(User userResponse)
        {
            if (userResponse.IsBlocked)
            {
                return new ServiceResponse<AuthorizationUserDTO>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.Auth.Locked,
                    Message = "Користувача заблоковано!"
                };
            }

            if (userResponse.IsDeleted)
            {
                return new ServiceResponse<AuthorizationUserDTO>()
                {
                    IsSuccess = false,
                    StatusCode = StatusCode.General.NotFound,
                    Message = "Користувач видалений!"
                };
            }

            return new ServiceResponse<AuthorizationUserDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Дійсний користувач!"
            };
        }

        private async Task<User> CreateNewUserEntityAsync(UserRegisterModel registrationModel)
        {
            string passwordSalt = Guid.NewGuid().ToString();
            string encryptionPassword = await _passwordHasher.HashPasswordAsync(registrationModel.Password, passwordSalt);

            return new User()
            {
                UserId = Guid.NewGuid(),
                Email = registrationModel.Email,
                PasswordHash = encryptionPassword,
                PasswordSalt = passwordSalt,
                Role = Role.User,
                CreatedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                IsBlocked = false,
                IsDeleted = false,
            };
        }
    }
}
