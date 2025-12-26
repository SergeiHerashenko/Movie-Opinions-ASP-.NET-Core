using AuthService.DAL.Interface;
using AuthService.Helpers;
using AuthService.Models.Enums;
using AuthService.Models.Responses;
using AuthService.Models.User;
using AuthService.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthService.Services.Implementations
{
    public class AuthServiceImplementations : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthServiceImplementations(IAuthRepository authRepository, HttpClient httpClient, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(UserLoginModel loginModel)
        {
            try
            {
                var getUser = await _authRepository.GetUserByEmailAsync(loginModel.Email);

                if(getUser.StatusCode != AuthStatusCode.UserFound)
                {
                    return new AuthResult()
                    {
                        IsSuccess = false,
                        Status = getUser.StatusCode,
                        Token = null,
                        Message = getUser.Message,
                    };
                }

                bool isPasswordCorrect = await new CheckingCorrectnessPassword().VerifyPasswordAsync(
                    loginModel.Password,
                    getUser.Data.PasswordHash,
                    getUser.Data.PasswordSalt);

                if (!isPasswordCorrect)
                {
                    return new AuthResult()
                    {
                        IsSuccess = false,
                        Status = AuthStatusCode.InvalidCredentials,
                        Token = null,
                        Message = "Невірний логін або пароль!"
                    };
                }

                var isAcces = CheckUserAccess(getUser.Data);

                return isAcces;
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Status = AuthStatusCode.InternalServerError,
                    Token = null,
                    RefreshToken = null,
                    ExpiryDate = null,
                    Message = ex.Message,
                    Errors = null
                };
            }
        }

        public async Task<AuthResult> RegistrationAsync(UserRegisterModel registrationModel)
        {
            try
            {
                // 1. Перевірка на існування
                var getUser = await _authRepository.GetUserByEmailAsync(registrationModel.Email);

                if(getUser.StatusCode != AuthStatusCode.UserNotFound)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Status = getUser.StatusCode,
                        Token = null,
                        RefreshToken = null,
                        ExpiryDate = null,
                        Message = getUser.ErrorMessage,
                        Errors = null
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
                var registerUser = await _authRepository.CreateUserAsync(newUser);

                if(registerUser.StatusCode != AuthStatusCode.UserCreated)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Status = registerUser.StatusCode,
                        Token = null,
                        RefreshToken = null,
                        ExpiryDate = null,
                        Message = registerUser.ErrorMessage,
                        Errors = null
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
                    await _authRepository.DeleteUserAsync(newUser.UserId);

                    var errorResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<Guid>>() 
                        ?? new ServiceResponse<Guid> { Message = "Сервіс профілів повернув помилку" };

                    return new AuthResult
                    {
                        IsSuccess = false,
                        Status = AuthStatusCode.InternalServerError,
                        Token = null,
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

                return new AuthResult
                {
                    IsSuccess = true,
                    Status = AuthStatusCode.Success,
                    Token = token,
                    Message = "Реєстрація успішна!"
                };
            }
            catch(Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Status = AuthStatusCode.InternalServerError,
                    Token = null,
                    RefreshToken = null,
                    ExpiryDate = null,
                    Message = ex.Message,
                    Errors = null
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

        private AuthResult CheckUserAccess(UserEntity userResponse)
        {
            if (userResponse.IsBlocked)
            {
                return new AuthResult()
                {
                    IsSuccess = false,
                    Status = AuthStatusCode.UserLockedOut,
                    Token = null,
                    Message = "Користувача заблоковано!"
                };
            }

            if (userResponse.IsDeleted)
            {
                return new AuthResult()
                {
                    IsSuccess = false,
                    Status = AuthStatusCode.UserDeleted,
                    Token = null,
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

            return new AuthResult()
            {
                IsSuccess = true,
                Status = AuthStatusCode.Success,
                Token = token
            };
        }
    }
}
