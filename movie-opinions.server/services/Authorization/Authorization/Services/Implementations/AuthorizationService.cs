using Authorization.DAL.Interface;
using Authorization.Helpers;
using Authorization.Models.Enums;
using Authorization.Models.Responses;
using Authorization.Models.User;
using Authorization.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Services.Implementations
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthorizationService(IAuthorizationRepository authorizationRepository, HttpClient httpClient, IConfiguration configuration)
        {
            _authorizationRepository = authorizationRepository;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthorizationResult> LoginAsync(UserLoginModel loginModel)
        {
            try
            {
                var getUser = await _authorizationRepository.GetUserByEmailAsync(loginModel.Email);

                if (getUser.StatusCode != AuthorizationStatusCode.UserFound)
                {
                    return new AuthorizationResult()
                    {
                        IsSuccess = false,
                        Status = getUser.StatusCode,
                        Message = getUser.Message,
                    };
                }

                bool isPasswordCorrect = await new CheckingCorrectnessPassword().VerifyPasswordAsync(
                    loginModel.Password,
                    getUser.Data.PasswordSalt,
                    getUser.Data.PasswordHash);

                if (!isPasswordCorrect)
                {
                    return new AuthorizationResult()
                    {
                        IsSuccess = false,
                        Status = AuthorizationStatusCode.InvalidCredentials,
                        Message = "Невірний логін або пароль!"
                    };
                }

                var isAcces = CheckUserAccess(getUser.Data);

                return isAcces;
            }
            catch (Exception ex)
            {
                return new AuthorizationResult
                {
                    IsSuccess = false,
                    Status = AuthorizationStatusCode.InternalServerError,
                    Message = ex.Message,
                };
            }
        }

        public async Task<AuthorizationResult> RegistrationAsync(UserRegisterModel registrationModel)
        {
            try
            {
                // 1. Перевірка на існування
                var getUser = await _authorizationRepository.GetUserByEmailAsync(registrationModel.Email);

                if (getUser.StatusCode != AuthorizationStatusCode.UserNotFound)
                {
                    return new AuthorizationResult
                    {
                        IsSuccess = false,
                        Status = getUser.StatusCode,
                        Message = getUser.Message,
                    };
                }

                // 2. Створення сутності
                string passwordSalt = Guid.NewGuid().ToString();
                string encryptionPassword = await new HashPassword().GetHashedPasswordAsync(registrationModel.Password, passwordSalt);

                var newUser = new UserEntity()
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

                // 3. Збереження в Auth базі
                var registerUser = await _authorizationRepository.CreateUserAsync(newUser);

                if (registerUser.StatusCode != AuthorizationStatusCode.UserCreated)
                {
                    return new AuthorizationResult
                    {
                        IsSuccess = false,
                        Status = registerUser.StatusCode,
                        Message = registerUser.Message,
                    };
                }

                // 4. HTTP виклик до ProfileService
                var profileData = new
                {
                    UserId = newUser.UserId,
                    Email = newUser.Email,
                };

                var response = await _httpClient.PostAsJsonAsync("api/profile", profileData);

                if (!response.IsSuccessStatusCode)
                {
                    // ROLLBACK: якщо профіль не створився — видаляємо юзера з Auth
                    await _authorizationRepository.DeleteUserAsync(newUser.UserId);

                    var errorResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<Guid>>()
                        ?? new ServiceResponse<Guid> { Message = "Сервіс профілів повернув помилку" };

                    return new AuthorizationResult
                    {
                        IsSuccess = false,
                        Status = AuthorizationStatusCode.InternalServerError,
                        Message = errorResponse?.Message ?? "Сервіс профілів повернув помилку"
                    };
                }

                var tokenModel = new UserTokenModel()
                {
                    UserId = registerUser.Data.UserId,
                    Email = registerUser.Data.Email,
                    IsEmailConfirmed = registerUser.Data.IsEmailConfirmed
                };

                var token = GenerateJwtToken(tokenModel);

                return new AuthorizationResult
                {
                    IsSuccess = true,
                    Status = AuthorizationStatusCode.Success,
                    Token = token,
                    Message = "Реєстрація успішна!"
                };
            }
            catch (Exception ex)
            {
                return new AuthorizationResult
                {
                    IsSuccess = false,
                    Status = AuthorizationStatusCode.InternalServerError,
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

        private AuthorizationResult CheckUserAccess(UserEntity userResponse)
        {
            if (userResponse.IsBlocked)
            {
                return new AuthorizationResult()
                {
                    IsSuccess = false,
                    Status = AuthorizationStatusCode.UserLockedOut,
                    Message = "Користувача заблоковано!"
                };
            }

            if (userResponse.IsDeleted)
            {
                return new AuthorizationResult()
                {
                    IsSuccess = false,
                    Status = AuthorizationStatusCode.UserDeleted,
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

            return new AuthorizationResult()
            {
                IsSuccess = true,
                Status = AuthorizationStatusCode.Success,
                Token = token
            };
        }
    }
}
