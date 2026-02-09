using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }
        // Змінити моделі бо одна не підходить для всього Створити окрему модель для підтвердження, щоб не ганяти ChangePasswordModel
        [Authorize]
        [HttpPost("change-password/request")]
        public async Task<IActionResult> RequestChange([FromBody] ChangePasswordModel model)
        {
            _logger.LogInformation("Спроба перевірки паролю користувача");

            try
            {
                var changeResponse = await _accountService.InitiatePasswordChangeAsync(model);

                if (!changeResponse.IsSuccess)
                {
                    _logger.LogWarning("Невірний старий пароль або помилка серверу!");

                    return BadRequest(changeResponse);
                }

                _logger.LogInformation("Старий пароль підтверджено!");

                return Ok(changeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Під час перевірки паролю сталася внутрішня помилка!");

                return StatusCode(500, "Під час перевірки паролю сталася внутрішня помилка!");
            }
        }

        [Authorize]
        [HttpPost("change-password/confirm")]
        public async Task<IActionResult> ConfirmChange([FromQuery] string code, [FromBody] ChangePasswordModel model)
        {
            _logger.LogInformation("Спроба зміни паролю користувача");

            try
            {
                var changeResponse = await _accountService.ChangePasswordAsync(code, model);

                if (!changeResponse.IsSuccess)
                {
                    _logger.LogWarning("Невірний код підтвердження або помилка серверу!");

                    return BadRequest(changeResponse);
                }

                _logger.LogInformation("Зміна паролю успішна!");

                return Ok(changeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Під час зміни паролю сталася внутрішня помилка!");

                return StatusCode(500, "Під час зміни паролю сталася внутрішня помилка!");
            }
        }
    }
}
