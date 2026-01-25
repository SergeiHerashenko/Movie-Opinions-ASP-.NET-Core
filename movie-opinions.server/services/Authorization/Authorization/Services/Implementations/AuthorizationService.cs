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

        public AuthorizationService(IAuthorizationRepository authorizationRepository, IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _authorizationRepository = authorizationRepository;
            _httpClientFactory = httpClient;
            _configuration = configuration;
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

                return isAcces;
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
                                // Поки не всі поля!
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
                    UserId = newUser.UserId,
                    Token = token,
                    // Додати інші поля (Поки не розумію)
                };

                return new ServiceResponse<AuthorizationUserDTO>
                {
                    IsSuccess = true,
                    StatusCode = StatusCode.General.Ok,
                    Data = userDTO,
                    Message = "Реєстрація успішна!"
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
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            // 4. Повертаємо серіалізований рядок
            return new JwtSecurityTokenHandler().WriteToken(token);
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

            var generateJWL = new UserTokenModel()
            {
                UserId = userResponse.UserId,
                Email = userResponse.Email,
                IsEmailConfirmed = userResponse.IsEmailConfirmed
            };

            var token = GenerateJwtToken(generateJWL);

            var userDTO = new AuthorizationUserDTO()
            {
                UserId = userResponse.UserId,
                Token = token,
                // Додати інші поля (Поки не розумію)
            };

            return new ServiceResponse<AuthorizationUserDTO>()
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Data = userDTO
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
