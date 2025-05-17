using Microsoft.AspNetCore.Mvc;

namespace OK.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Game()
        {
            return View();
        }
    }
}
