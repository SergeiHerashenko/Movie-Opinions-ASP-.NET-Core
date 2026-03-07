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
    }
}
