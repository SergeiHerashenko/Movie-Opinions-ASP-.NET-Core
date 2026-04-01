using Authorization.Application.DTO.Authentication.Commands;
using Authorization.Application.Interfaces.Services;
using Authorization.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            var command = new LoginCommand()
            {
                Login = userLoginRequest.Login,
                Password = userLoginRequest.Password
            };

            var result = await _authorizationService.LoginAsunc(command);

            // Заглушка поки
            return Ok(result);
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            var command = new RegistrationCommand()
            {
                Login = userRegistrationRequest.Login,
                Password = userRegistrationRequest.Password
            };

            var result = await _authorizationService.RegistrationAsync(command);

            // Заглушка поки
            return Ok(result);
        }
    }
}
