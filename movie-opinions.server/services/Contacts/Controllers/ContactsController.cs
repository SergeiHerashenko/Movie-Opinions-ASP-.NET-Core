using Microsoft.AspNetCore.Mvc;

namespace Contacts.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        [HttpGet("contacts/{contractsId}")]
        public async Task<IActionResult> GetContact()
        {
            return Ok();
        }
    }
}
