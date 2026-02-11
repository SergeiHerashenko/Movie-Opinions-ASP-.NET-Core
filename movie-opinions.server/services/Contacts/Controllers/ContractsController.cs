using Microsoft.AspNetCore.Mvc;

namespace Contacts.Controllers
{
    [ApiController]
    [Route("api/contracts")]
    public class ContractsController : ControllerBase
    {
        [HttpGet("contracts/{contractsId}")]
        public async Task<IActionResult> GetContact()
        {
            
        }
    }
}
