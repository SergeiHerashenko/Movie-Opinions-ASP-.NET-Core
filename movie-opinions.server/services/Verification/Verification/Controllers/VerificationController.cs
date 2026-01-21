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

        [HttpPost("verification")]
        public async Task<IActionResult> GenerateToken()
        {
            var getToken = await _verificationService.GenerateVerificationToken();
            return Ok(getToken);
        }
    }
}
