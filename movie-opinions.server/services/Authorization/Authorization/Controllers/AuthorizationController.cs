using Authorization.Application.Interfaces;
using Authorization.Models.User;
using Microsoft.AspNetCore.Mvc;
using AspNetAuth = Microsoft.AspNetCore.Authorization;
using Status = MovieOpinions.Contracts.Models.StatusCode;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            try
            {
                var result = await _authorizationService.RegistrationAsync(model);

                if (!result.IsSuccess)
                {
                    return result.StatusCode == Status.Create.Conflict
                        ? Conflict(result)
                        : BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Під час реєстрації сталася внутрішня помилка!");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            try
            {
                var result = await _authorizationService.LoginAsync(model);

                if (!result.IsSuccess)
                {
                    return (int)result.StatusCode switch
                    {
                        Status.Auth.Unauthorized => Unauthorized(result),
                        Status.Auth.Locked => StatusCode(423, result),
                        Status.Verification.Expired => StatusCode(410, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Під час входу сталася внутрішня помилка!");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authorizationService.LogoutAsync();
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var result = await _authorizationService.RefreshTokenAsync();

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        [AspNetAuth.Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var change = await _authorizationService.ChangePasswordAsync();

                if (!change.IsSuccess)
                {
                    return change.StatusCode == Status.Create.Conflict
                        ? Conflict(change)
                        : BadRequest(change);
                }

                return Ok(change);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Під час зміни паролю сталась внутрішня помилка!");
            }
        }
    }
}