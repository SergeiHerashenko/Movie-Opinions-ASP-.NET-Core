using Microsoft.AspNetCore.Mvc;
using Profile.Models.Profile;
using Profile.Services.Interfaces;

namespace Profile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileDTO model)
        {
            var resultCreate = await _profileService.CreateProfileAsync(model);

            if (!resultCreate.IsSuccess)
                return BadRequest(resultCreate);

            return Ok(resultCreate);
        }
    }
}
