using AuthService.DAL.Interface;
using AuthService.Helpers;
using AuthService.Models.Enums;
using AuthService.Models.Responses;
using AuthService.Models.User;
using AuthService.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace AuthService.Services.Implementations
{
    public class AuthServiceImplementations : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthServiceImplementations(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<AuthResult> LoginAsync(UserLoginModel loginModel)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthResult> RegistrationAsync(UserRegisterModel registrationModel)
        {
            try
            {
                var getUser = await _authRepository.GetUserByEmailAsync(registrationModel.Email);

                if(getUser.StatusCode == AuthStatusCode.UserNotFound)
                {
                    string passwordSalt = Guid.NewGuid().ToString();
                    string encryptionPassword = await new HashPassword().GetHashedPasswordAsync(registrationModel.Password, passwordSalt);

                    var newUser = new UserEntity()
                    {
                        UserId = Guid.NewGuid(),
                        Email = registrationModel.Email,
                        Password = encryptionPassword,
                        SaltPassword = passwordSalt
                    };

                    var registerUser = await _authRepository.RegistrationUserAsync(newUser);

                    if(registerUser.StatusCode == AuthStatusCode.Success)
                    {
                        using(var client = new HttpClient())
                        {
                            var profileData = new
                            {
                                UserId = newUser.UserId,
                                Email = newUser.Email
                            };

                            var content = new StringContent(
                                JsonSerializer.Serialize(profileData),
                                Encoding.UTF8,
                                "application/json"
                            );

                            var response = await client.PostAsync("https://profileservice/api/profiles", content);
                            
                            if (!response.IsSuccessStatusCode)
                            {
                                return new AuthResult
                                {
                                    IsSuccess = false,
                                    Status = AuthStatusCode.InvalidCredentials,
                                    Token = null,
                                    RefreshToken = null,
                                    ExpiryDate = null,
                                    Message = getUser.ErrorMessage,
                                    Errors = null
                                };
                            }

                            return new AuthResult
                            {
                                IsSuccess = true,
                                Status = AuthStatusCode.Success,
                                Message = "Реєстрація успішна!"
                            };
                        }
                    }
                }

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
            catch(Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Status = AuthStatusCode.UserAlreadyExists,
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
