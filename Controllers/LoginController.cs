using Microsoft.AspNetCore.Mvc;
using OK.Data;
using OK.Models;
using OK.Helpers;

namespace OK.Controllers
{
    public class LoginController : Controller
    {

        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario usuario)
        {
            // Verificar que el correo electrónico no esté en uso
            var existingUser = _context.Usuarios.FirstOrDefault(u => u.Email == usuario.Email);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Este correo ya está registrado";
                return View();
            }

            // Crear y guardar el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = usuario.Nombre,
                ApPat = usuario.ApPat,
                ApMat = usuario.ApMat,
                Email = usuario.Email,
                Edad = usuario.Edad,
                Sexo = usuario.Sexo,
                Password = CifradoHelper.Hash(usuario.Password), // Aquí deberías cifrar la contraseña antes de guardarla
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            // Iniciar sesión automáticamente
            // Aquí puedes configurar la sesión del usuario si es necesario.
            HttpContext.Session.SetString("UserId", nuevoUsuario.Id.ToString());

            // Redirigir al perfil del usuario o a la página principal
            return RedirectToAction("Perfil", "Usuarios");

        }

    }
}
