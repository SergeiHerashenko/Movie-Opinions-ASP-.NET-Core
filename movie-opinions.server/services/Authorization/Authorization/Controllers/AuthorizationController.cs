using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Request;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Status = MovieOpinions.Contracts.Models.StatusCode;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(IAuthorizationService authorizationService, ILogger<AuthorizationController> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            _logger.LogInformation("Спроба створити користувача з Email: {Email}", model.Email);

            try
            {
                var registerUser = await _authorizationService.RegisterAsync(model);

                if (!registerUser.IsSuccess)
                {
                    _logger.LogWarning("Користувач {Email} не зареєстрований. Виникла помилка з кодом {StatusCode}.", model.Email, registerUser.StatusCode);

                    return registerUser.StatusCode == Status.Create.Conflict
                        ? Conflict(registerUser)
                        : BadRequest(registerUser);
                }

                _logger.LogInformation("Користувач {Email} зареєстрований", model.Email);

                return Ok(registerUser);
            }
            catch (Exception ex)
            {
                _logger.LogError("Під час реєстрації сталася внутрішня помилка! {ex}", ex);

                return StatusCode(500, "Під час реєстрації сталася внутрішня помилка!");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            _logger.LogInformation("Спроба входу на сайт користувача {Email}", model.Email);

            try
            {
                var loginUser = await _authorizationService.LoginAsync(model);

                if (!loginUser.IsSuccess)
                {
                    _logger.LogWarning("Сталася помилка при авторизації користувача!");

                    return (int)loginUser.StatusCode switch
                    {
                        Status.Auth.Unauthorized => Unauthorized(loginUser),
                        Status.Auth.Locked => StatusCode(423, loginUser),
                        Status.Verification.Expired => StatusCode(410, loginUser),
                        _ => BadRequest(loginUser)
                    };
                }

                _logger.LogInformation("Вхід успішний!");

                return Ok(loginUser);
            }
            catch (Exception ex)
            {
                _logger.LogError("Під час авторизації сталася внутрішня помилка! {ex}", ex);

                return StatusCode(500, "Під час авторизації сталася внутрішня помилка!");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Спроба входу на сайт користувача!");

            try
            {
                var logoutUser = await _authorizationService.LogoutAsync();

                if (!logoutUser.IsSuccess)
                {
                    _logger.LogWarning("Сталася помилка при виході користувача!");

                    // Додати точні помилки 
                    return BadRequest(logoutUser);
                }

                _logger.LogInformation("Вихід успішний!");

                return Ok(logoutUser);
            }
            catch (Exception ex)
            {
                _logger.LogError("Під час виходу з системи сталася внутрішня помилка! {ex}", ex);

                return StatusCode(500, "Під час виходу з системи сталася внутрішня помилка!");
            }
        }
    }
}