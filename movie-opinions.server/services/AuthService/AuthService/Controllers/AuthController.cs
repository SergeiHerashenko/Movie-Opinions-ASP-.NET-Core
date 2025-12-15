using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
