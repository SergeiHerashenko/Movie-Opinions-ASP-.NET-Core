using Microsoft.AspNetCore.Mvc;
using Verification.Services.Interfaces;

namespace Verification.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        
        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken([FromBody] Guid userId)
        {
            var getToken = await _verificationService.GenerateVerificationToken(userId);
            return Ok(getToken);
        }

        [HttpPost("code")]
        public async Task<IActionResult> GenerateCode()
        {
            var getCode = await _verificationService.GenerateVerificationCode();
            return Ok(getCode);
        }
    }
}
