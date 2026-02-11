using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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
        [HttpPost("initiate-pass-change")]
        public async Task<IActionResult> InitiatePasswordChangeAsync ([FromBody] ChangePasswordModel model)
        {
            _logger.LogInformation("Спроба перевірки паролю користувача");

            try
            {
                var changeResponse = await _accountService.InitiateAccountChange(model);

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
        [HttpPost("send-confirm")]
        public async Task<IActionResult> SendingConfirmation([FromBody] SendVerificationCodeRequest request)
        {
            _logger.LogInformation("Спроба відправки листа підтвердження!");

            try
            {
                var sendResponse = await _accountService.SendingConfirmationAsync(request);

                if (!sendResponse.IsSuccess)
                {
                    _logger.LogWarning("Помилка при відправлені листа підтвердження!");

                    return BadRequest(sendResponse);
                }

                _logger.LogInformation("Лист підтвердження відправлено!");

                return Ok(sendResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Під час відправки листа підтвердження сталася внутрішня помилка!");

                return StatusCode(500, "Під час відправки листа підтвердження сталася внутрішня помилка!");
            }
        }
    }
}
