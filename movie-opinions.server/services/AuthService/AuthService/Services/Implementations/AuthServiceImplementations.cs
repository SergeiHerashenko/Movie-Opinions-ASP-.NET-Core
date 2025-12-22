using AuthService.DAL.Interface;
using AuthService.Helpers;
using AuthService.Models.Enums;
using AuthService.Models.Responses;
using AuthService.Models.User;
using AuthService.Services.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AuthService.Services.Implementations
{
    public class AuthServiceImplementations : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly HttpClient _httpClient;

        public AuthServiceImplementations(IAuthRepository authRepository, HttpClient httpClient)
        {
            _authRepository = authRepository;
            _httpClient = httpClient;
        }

        public async Task<AuthResult> LoginAsync(UserLoginModel loginModel)
        {
            throw new NotImplementedException();
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
                        Status = AuthStatusCode.UserAlreadyExists,
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
                    Password = encryptionPassword,
                    SaltPassword = passwordSalt
                };

                // 3. Збереження в Auth базі
                var registerUser = await _authRepository.RegistrationUserAsync(newUser);

                if(registerUser.StatusCode != AuthStatusCode.Success)
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
                    Email = newUser.Email 
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
                        Message = errorResponse?.Message ?? "Сервіс профілів повернув помилку"
                    };
                }

                return new AuthResult
                {
                    IsSuccess = true,
                    Status = AuthStatusCode.Success,
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

        public string GenerateJwtToken(UserTokenModel user)
        {
            throw new NotImplementedException();
        }
    }
}
