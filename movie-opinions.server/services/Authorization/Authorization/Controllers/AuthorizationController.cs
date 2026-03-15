using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.Interfaces.Services;
using Contracts.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Auth = Microsoft.AspNetCore.Authorization;
using Status = Contracts.Models.Status.StatusCode;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(IAuthorizationService authorizationService,
            ILogger<AuthorizationController> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
        {
            _logger.LogInformation("Реєстрація нового користувача: {Login}", userRegistrationDTO.Login);
            
            var result = await _authorizationService.RegistrationAsync(userRegistrationDTO);

            return HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
        {
            _logger.LogInformation("Вхід користувача: {Login}", userLoginDTO.Login);

            var result = await _authorizationService.LoginAsync(userLoginDTO);

            return HandleResult(result);
        }

        [Auth.Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Вихід користувача");

            var result = await _authorizationService.LogoutAsync();

            return HandleResult(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshSessionAsync()
        {
            _logger.LogInformation("Оновлення токену користувача");

            var result = await _authorizationService.RefreshSessionAsync();

            return HandleResult(result);
        }

        [NonAction]
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            int code = result.StatusCode;

            return code switch
            {
                // Авторизація
                Status.Auth.Unauthorized => Unauthorized(result),
                Status.Auth.Forbidden => StatusCode(403, result),
                Status.Auth.Locked => StatusCode(423, result),
                Status.Auth.Deleted => StatusCode(410, result),

                // Загальні
                Status.General.NotFound => NotFound(result),
                Status.General.BadRequest => BadRequest(result),
                Status.General.InternalError => StatusCode(500, result),

                // Створення та Конфлікти
                Status.Create.Conflict => Conflict(result),

                // Верифікація
                Status.Verification.Expired => StatusCode(498, result),
                Status.Verification.Invalid => UnprocessableEntity(result),

                // Значення за замовчуванням
                _ => StatusCode(500, result)
            };
        }
    }
}
