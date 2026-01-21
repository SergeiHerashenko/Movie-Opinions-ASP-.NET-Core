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

        [HttpGet("templates")]
        public async Task<IActionResult> Index(string templateName)
        {
            var getTemplate = await _templateService.GetTemplateText(templateName);

            return Ok(getTemplate);
        }
    }
}
