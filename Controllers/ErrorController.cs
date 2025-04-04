using Microsoft.AspNetCore.Mvc;

namespace library_management.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UnauthorisedAccess()
        {
            return View();
        }
    }
}
