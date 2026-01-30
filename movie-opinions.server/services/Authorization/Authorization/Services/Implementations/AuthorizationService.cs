using Authorization.DAL.Interface;
using Authorization.Helpers;
using Authorization.Models.User;
using Authorization.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MovieOpinions.Contracts.Enum;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;
using MovieOpinions.Contracts.Models.ServiceResult;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Services.Implementations
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IAuthorizationRepository authorizationRepository, IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationRepository = authorizationRepository;
            _httpClientFactory = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

                bool isPasswordCorrect = await new CheckingCorrectnessPassword().VerifyPasswordAsync(
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
                    Body = new { UserId = newUser.UserId, Email = newUser.Email }
                };

                var responseProfile = await SendInternalRequest<object, Guid>(profileRequest);

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

                var responseNotification = await SendInternalRequest<object, object>(notificationRequest);

                var tokenModel = new UserTokenModel()
                {
                    UserId = registerUser.Data.UserId,
                    Email = registerUser.Data.Email,
                    IsEmailConfirmed = registerUser.Data.IsEmailConfirmed
                };

                var token = GenerateJwtToken(tokenModel);

                var userDTO = new AuthorizationUserDTO()
                {
                    UserId = registerUser.Data.UserId,
                    Token = token,
                    // Додати інші поля (Поки не розумію)
                };

                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Data = userDTO,
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

        public async Task<ServiceResponse<AuthorizationUserDTO>> RefreshTokenAsync()
        {
            var oldAccessToken = _httpContextAccessor.HttpContext.Request.Cookies["jwt"];
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["X-Refresh-Token"];

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(oldAccessToken))
                return new ServiceResponse<AuthorizationUserDTO> { IsSuccess = false, Message = "Сесія втрачена" };

            // 1. Отримуємо Claims із простроченого токена
            var principal = GetPrincipalFromExpiredToken(oldAccessToken);
            var userIdStr = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
                return new ServiceResponse<AuthorizationUserDTO> { IsSuccess = false, Message = "Невалідний токен" };

            // 2. Шукаємо токен у базі
            var userToken = await _authorizationRepository.GetTokenAsync(refreshToken, userId);

            // 3. Перевіряємо валідність
            if (userToken.Data == null || userToken.Data.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                return new ServiceResponse<AuthorizationUserDTO> { IsSuccess = false, Message = "Сесія прострочена" };
            }

            // 4. Шукаємо самого юзера, щоб створити йому нову сесію
            var user = await _authorizationRepository.GetUserByIdAsync(userId);

            // 5. Просто викликаємо твій готовий метод!
            return await CreateUserSessionAsync(user.Data);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Невалідний токен");

            return principal;
        }

        private string GenerateJwtToken(UserTokenModel user)
        {
            // 1. Створюємо список Claims
            var claims = new List<Claim>
            {
                // Використовуємо JwtRegisteredClaimNames для стандартизації
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Унікальний ID токена
        
                new Claim("email_confirmed", user.IsEmailConfirmed.ToString().ToLower()),

                new Claim(ClaimTypes.Role, "User")
            };

            // 2. Отримуємо ключ із конфігурації
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new Exception("Критична помилка: JWT Key занадто короткий або відсутній!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Створюємо сам токен
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            // 4. Повертаємо серіалізований рядок
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private async Task<ServiceResponse<AuthorizationUserDTO>> CreateUserSessionAsync(UserEntity user)
        {
            // 1. Готуємо модель для JWT
            var tokenModel = new UserTokenModel()
            {
                UserId = user.UserId,
                Email = user.Email,
                IsEmailConfirmed = user.IsEmailConfirmed,
            };

            // 2. Генеруємо токени
            var accessToken = GenerateJwtToken(tokenModel);
            var refreshToken = GenerateRefreshToken();

            SetCookie("jwt", accessToken, DateTime.UtcNow.AddMinutes(15));

            SetCookie("X-Refresh-Token", refreshToken, DateTime.UtcNow.AddDays(7));

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

            // 5. Повертаємо готовий DTO
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

        private void SetCookie(string name, string value, DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
                Path = "/" // Важливо, щоб кука була доступна для всіх запитів
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(name, value, options);
        }

        private ServiceResponse<AuthorizationUserDTO> CheckUserAccess(UserEntity userResponse)
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

        private async Task<UserEntity> CreateNewUserEntityAsync(UserRegisterModel registrationModel)
        {
            string passwordSalt = Guid.NewGuid().ToString();
            string encryptionPassword = await new HashPassword().GetHashedPasswordAsync(registrationModel.Password, passwordSalt);

            return new UserEntity()
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

        private async Task<ServiceResponse<TResponse>> SendInternalRequest<TBody, TResponse>(InternalRequest<TBody> internalReques)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(internalReques.ClientName);
                HttpResponseMessage response;

                if (internalReques.Method == HttpMethod.Post)
                {
                    response = await client.PostAsJsonAsync(internalReques.Endpoint, internalReques.Body);
                }
                else
                {
                    response = await client.GetAsync(internalReques.Endpoint);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TResponse>();

                    return new ServiceResponse<TResponse>
                    {
                        IsSuccess = true,
                        Data = result,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var errorData = await response.Content.ReadFromJsonAsync<ServiceResult<object>>();

                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = errorData?.Message ?? response.ReasonPhrase,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TResponse>
                {
                    IsSuccess = false,
                    Message = $"Критична помилка: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
