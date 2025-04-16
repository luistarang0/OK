using Microsoft.AspNetCore.Mvc;

namespace OK.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Perfil()
        {
            return View();
        }
    }
}
