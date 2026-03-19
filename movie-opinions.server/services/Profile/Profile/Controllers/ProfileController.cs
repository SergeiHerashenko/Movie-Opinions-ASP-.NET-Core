using Contracts.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Profile.Application.Interfaces;
using Status = Contracts.Models.Status.StatusCode;

namespace Profile.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileService profileService,
            ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize(Policy = "ServiceProfileCreate")]
        public async Task<IActionResult> CreateProfile()
        {
            _logger.LogInformation("Створення профілю користувача");

            var result = await _profileService.ProfileCreateAsync();

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
