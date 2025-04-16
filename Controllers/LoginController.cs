using Microsoft.AspNetCore.Mvc;

namespace OK.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Registro()
        {
            return View();
        }
    }
}
