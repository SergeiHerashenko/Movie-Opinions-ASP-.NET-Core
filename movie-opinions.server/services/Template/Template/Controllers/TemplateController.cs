using Microsoft.AspNetCore.Mvc;
using Template.Services.Interfaces;

namespace Template.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet("templates/{nameTemplate}")]
        public async Task<IActionResult> GetTemplate(string nameTemplate)
        {
            var getTemplate = await _templateService.GetTemplateText(nameTemplate);

            return Ok(getTemplate);
        }
    }
}
