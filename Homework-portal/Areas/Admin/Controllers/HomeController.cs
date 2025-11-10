using Microsoft.AspNetCore.Authorization; // 1. BUNU EKLEYİN
using Microsoft.AspNetCore.Mvc;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // 2. BUNU EKLEYİN (Artık bu controller'a giriş yapmadan erişilemez)
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}