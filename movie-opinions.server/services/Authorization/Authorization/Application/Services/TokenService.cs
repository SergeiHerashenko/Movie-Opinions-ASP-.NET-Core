using Authorization.Application.Interfaces.Cookie;
using Authorization.Application.Interfaces.Identity;
using Authorization.Application.Interfaces.Services;
using Authorization.DAL.Interface;
using Authorization.Domain.DTO;
using Authorization.Domain.Entities;
using Authorization.Domain.Models;
using MovieOpinions.Contracts.Models;
using MovieOpinions.Contracts.Models.ServiceResponse;

namespace Authorization.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IJwtProvider _jwtProvider;
        private readonly ICookieProvider _cookieProvider;
        private readonly IUserTokenRepository _userTokenRepository;

        public TokenService(ILogger<TokenService> logger,
            IJwtProvider jwtProvider,
            ICookieProvider cookieProvider,
            IUserTokenRepository userTokenRepository)
        {
            _logger = logger;
            _jwtProvider = jwtProvider;
            _cookieProvider = cookieProvider;
            _userTokenRepository = userTokenRepository;
        }

        public async Task<ServiceResponse<UserResponseDTO>> CreateUserSessionAsync(User user)
        {
            _logger.LogInformation("Формування JWT токену!");

            // 1. Готуємо модель для JWT
            var tokenModel = new UserClaimsModel()
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
            var userToken = new UserToken()
            {
                IdToken = Guid.NewGuid(),
                IdUser = user.UserId,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            };

            // Додати видалення інших токенів користувача
            var saveResult = await _userTokenRepository.CreateAsync(userToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Помилка збереження токену!");

                return new ServiceResponse<UserResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = saveResult.StatusCode,
                    Message = "Помилка при збереженні сесії."
                };
            }

            _logger.LogInformation("Токен успішно створений!");

            // 4. Повертаємо готовий DTO
            return new ServiceResponse<UserResponseDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Data = new UserResponseDTO
                {
                    IdUser = user.UserId,
                    Email = user.Email,
                    Role = user.Role
                },
                Message = "Вхід успішний!"
            };
        }

        public async Task<ServiceResponse<UserResponseDTO>> ClearCookies()
        {
            var refreshToken = _cookieProvider.GetCookie("X-Refresh-Token");

            var getToken = await _userTokenRepository.GetUserTokenAsync(refreshToken);

            if(getToken.StatusCode != StatusCode.General.Ok)
            {
                _logger.LogError("СТалася помилка при отриманні токену");

                _cookieProvider.ClearAuthCookies();

                return new ServiceResponse<UserResponseDTO>()
                {
                    IsSuccess = false,
                    StatusCode = getToken.StatusCode,
                    Message = getToken.Message
                };
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var deleteToken =  await _userTokenRepository.DeleteAsync(getToken.Data.IdToken);

                if(deleteToken.StatusCode != StatusCode.General.Ok)
                {
                    _logger.LogError("СТалася помилка при видалення токену");

                    _cookieProvider.ClearAuthCookies();

                    return new ServiceResponse<UserResponseDTO>()
                    {
                        IsSuccess = false,
                        StatusCode = getToken.StatusCode,
                        Message = getToken.Message
                    };
                }
            }

            _cookieProvider.ClearAuthCookies();

            return new ServiceResponse<UserResponseDTO>()
            {
                IsSuccess = true,
                StatusCode = StatusCode.General.Ok,
                Message = "Токен видалений!"
            };
        }
    }
}
