using Microsoft.AspNetCore.Mvc;
using ProfileService.Models.Profile;
using ProfileService.Services.Interfaces;

namespace ProfileService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public ProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileDTO model)
        {
            var resultCreate = await _userProfileService.CreateProfileAsync(model);

            if (!resultCreate.IsSuccess)
                return BadRequest(resultCreate);

            return Ok(resultCreate);
        }
    }
}
