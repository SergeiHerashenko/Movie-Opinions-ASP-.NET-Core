using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Http;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Entities;
using Contracts.Enum;
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
            // 1. Генеруємо токени
            var accessToken = _jwtProvider.GenerateAccessToken(userSessionDTO);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            _cookieProvider.SetAuthCookies(accessToken, refreshToken);

            // 2. Формуємо сутність для БД
            var userToken = new UserToken()
            {
                Id = Guid.NewGuid(),
                UserId = userSessionDTO.UserId,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            };

            // 3. Збереження токену в базу даних
            await _userTokenRepository.CreateAsync(userToken);

            _logger.LogInformation("Сесію створено для користувача {UserId}", userSessionDTO.UserId);

            return true;
        }

        public async Task<UserTokenDTO?> ValidateRefreshTokenAsync()
        {
            _logger.LogInformation("Перевірка валідності Refresh токена");

            // 1. Перевіряємо, чи взагалі є токен у куках
            var cookieToken = _cookieProvider.GetCookie("X-Refresh-Token");

            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogWarning("Refresh токен відсутній у куках або не збігається з наданим");

                return null;
            }

            // 2. Шукаємо токен у базі даних
            var tokenResult = await _userTokenRepository.GetUserTokenAsync(cookieToken);

            // 3. Якщо токена немає в БД — він невалідний
            if (tokenResult == null)
            {
                _logger.LogWarning("Токен не знайдено в базі даних");

                return null;
            }

            // 4. Перевіряємо термін дії
            if (tokenResult.RefreshTokenExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("Термін дії токена {TokenId} закінчився. Видалення...", tokenResult.Id);

                await _userTokenRepository.DeleteAsync(tokenResult.Id);

                return null;
            }

            return new UserTokenDTO()
            {
                UserId = tokenResult.UserId
            };
        }

        public async Task<bool> DeleteSessionAsync()
        {
            var cookieToken = _cookieProvider.GetCookie("X-Refresh-Token");

            if (string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogWarning("Refresh токен відсутній у куках або не збігається з наданим");

                _cookieProvider.ClearAuthCookies();

                return false;
            }

            var tokenEntry = await _userTokenRepository.GetUserTokenAsync(cookieToken);

            if (tokenEntry != null)
            {
                await _userTokenRepository.DeleteAsync(tokenEntry.Id);

                _logger.LogInformation("Сесію {TokenId} видалено з бази", tokenEntry.Id);
            }

            _cookieProvider.ClearAuthCookies();

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
