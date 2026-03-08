using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Http;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly ICookieProvider _cookieProvider;
        private readonly IJwtProvider _jwtProvider;

        public TokenService(IUserTokenRepository userTokenRepository,
            ICookieProvider cookieProvider,
            ILogger<TokenService> logger,
            IJwtProvider jwtProvider)
        {
            _userTokenRepository = userTokenRepository;
            _cookieProvider = cookieProvider;
            _logger = logger;
            _jwtProvider = jwtProvider;
        }

        public async Task<bool> CreateUserSessionAsync(UserSessionDTO userSessionDTO)
        {
            // 1. Готуємо модель для JWT
            var tokenModel = new UserClaimsDTO()
            {
                UserId = userSessionDTO.UserId,
                Login = userSessionDTO.Login,
                Role = userSessionDTO.Role,
                IsEmailConfirmed = userSessionDTO.IsEmailConfirmed
            };

            // 2. Генеруємо токени
            var accessToken = _jwtProvider.GenerateAccessToken(tokenModel);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            _cookieProvider.SetAuthCookies(accessToken, refreshToken);

            // 3. Формуємо сутність для БД
            var userToken = new UserToken()
            {
                Id = Guid.NewGuid(),
                UserId = userSessionDTO.UserId,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            };

            // 4. Збереження токену в базу даних
            await _userTokenRepository.CreateAsync(userToken);

            _logger.LogInformation("Сесію створено для користувача {UserId}", userSessionDTO.UserId);

            return true;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            _logger.LogInformation("Перевірка валідності Refresh токена");

            // 1. Перевіряємо, чи взагалі є токен у куках
            var cookieToken = _cookieProvider.GetCookie("X-Refresh-Token");

            if (string.IsNullOrEmpty(cookieToken) || cookieToken != token)
            {
                _logger.LogWarning("Refresh токен відсутній у куках або не збігається з наданим");

                return false;
            }

            // 2. Шукаємо токен у базі даних
            var tokenResult = await _userTokenRepository.GetUserTokenAsync(token);

            // 3. Якщо токена немає в БД — він невалідний
            if (tokenResult == null)
            {
                _logger.LogWarning("Токен не знайдено в базі даних");

                return false;
            }

            // 4. Перевіряємо термін дії
            if (tokenResult.RefreshTokenExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("Термін дії токена {TokenId} закінчився. Видалення...", tokenResult.Id);

                await _userTokenRepository.DeleteAsync(tokenResult.Id);

                return false;
            }

            return true;
        }

        public async Task<bool> DeleteSessionAsync(string token)
        {
            var getToken = await _userTokenRepository.GetUserTokenAsync(token);

            _cookieProvider.ClearAuthCookies();

            if (getToken != null)
            {
                await _userTokenRepository.DeleteAsync(getToken.Id);

                return true;
            }

            return true;
        }

        public async Task ClearAllUserSessionsAsync(Guid userId)
        {
            var getAllTokenUser = await _userTokenRepository.GetAllTokensUserAsync(userId);

            foreach(var tokenUser in getAllTokenUser)
            {
                await _userTokenRepository.DeleteAsync(tokenUser.Id);
            }

            _cookieProvider.ClearAuthCookies();
        }
    }
}
