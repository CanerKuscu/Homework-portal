using Microsoft.AspNetCore.Mvc;

namespace Homework_portal.Controllers
{
    public class ErrorController : Controller
    {
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}