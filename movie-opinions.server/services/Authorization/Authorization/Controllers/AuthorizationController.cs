using Authorization.Application.DTO.Authentication.Request;
using Authorization.Application.DTO.Users;
using Authorization.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<UserResponseDTO> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
        {
            return await _authorizationService.RegistrationAsync(userRegistrationDTO);
        }

        [HttpPost("login")]
        public async Task<UserResponseDTO> Login([FromBody] UserLoginDTO userLoginDTO)
        {
            return await _authorizationService.LoginAsync(userLoginDTO);
        }

        [HttpPost("logout")]
        public async Task<UserResponseDTO> Logout()
        {
            return await _authorizationService.LogoutAsync();
        }

        [HttpPost("refresh-token")]
        public async Task<UserResponseDTO> RefreshSessionAsync()
        {
            return await _authorizationService.RefreshSessionAsync();
        }
    }
}
