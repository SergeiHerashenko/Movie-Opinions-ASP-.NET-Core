using Authorization.Application.DTO.Users;
using Authorization.Application.DTO.Users.Change;
using Authorization.Application.DTO.Users.Response;
using Authorization.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("change-password")]
        public async Task<ChangeResponseDTO> PasswordChangeAsync(ChangePasswordDTO changePasswordDTO)
        {
            _logger.LogInformation("Ініціація зміни паролю користувача");
            return await _accountService.ChangePasswordAsync(changePasswordDTO);
        }
    }
}
