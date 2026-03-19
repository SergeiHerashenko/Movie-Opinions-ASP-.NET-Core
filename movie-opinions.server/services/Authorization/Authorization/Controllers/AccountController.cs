using Authorization.Application.DTO.Users.Change;
using Authorization.Application.Interfaces.Services;
using Contracts.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Status = Contracts.Models.Status.StatusCode;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthorizationController> _logger;

        public AccountController(IAccountService accountService,
            ILogger<AuthorizationController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("initiate-password-change")]
        public async Task<IActionResult> InitiatePasswordChange(InitiatePasswordChangeDTO initiatePasswordChangeDTO)
        {
            _logger.LogInformation("Ініціація зміни паролю користувача");

            var result =  await _accountService.InitiatePasswordChangeAsync(initiatePasswordChangeDTO);

            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("send-code-change-password")]
        public async Task<IActionResult> SendCodeChangePassword(SendVerificationCodeDTO sendVerificationCodeDTO)
        {
            _logger.LogInformation("Відправка коду підтвердження!");

            var result = await _accountService.SendVerificationCodeAsync(sendVerificationCodeDTO);

            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("confirm-password-change")]
        public async Task<IActionResult> ConfirmPasswordChange(PasswordConfirmationDTO passwordConfirmationDTO)
        {
            _logger.LogInformation("Зміна паролю!");

            var result = await _accountService.ConfirmPasswordChangeAsync(passwordConfirmationDTO);

            return HandleResult(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string login)
        {
            _logger.LogInformation("Відновлення паролю!");

            var result = await _accountService.ResetPasswordAsync(login);

            return HandleResult(result);
        }

        [NonAction]
        protected IActionResult HandleResult(Result result)
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
