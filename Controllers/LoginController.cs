using Microsoft.AspNetCore.Mvc;
using OK.Data;
using OK.Models;
using OK.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
        public async Task<IActionResult> Login(string email, string password)
        {
            // Buscar el usuario en la base de datos por correo electrónico
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                // Si no se encuentra el usuario, mostrar un mensaje de error
                ViewBag.ErrorMessage = "El correo electrónico no está registrado.";
                return View();
            }

            // Verificar si la contraseña es correcta
            if (!CifradoHelper.VerifyPassword(password, usuario.Password))
            {
                // Si la contraseña no es válida, mostrar un mensaje de error
                ViewBag.ErrorMessage = "La contraseña es incorrecta.";
                return View();
            }

            // Crear los claims para el usuario autenticado
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // ID del usuario
                    new Claim(ClaimTypes.Name, usuario.Nombre), // Nombre del usuario
                    new Claim(ClaimTypes.Email, usuario.Email) // Correo electrónico
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Iniciar sesión (guardar la información del usuario en las cookies)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            // Iniciar sesión (guardar el id del usuario en la sesión)
            HttpContext.Session.SetString("UserId", usuario.Id.ToString());

            // Redirigir al perfil del usuario o a la página principal
            return RedirectToAction("GuardarSesion", "Sisco");
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

            int lastUserId = _context.Usuarios.Max(u => (int?)u.Id) ?? 0;  // Si no hay registros, se asigna 0
            usuario.Id = lastUserId + 1;  // Asignar el siguiente id disponible

            // Crear y guardar el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Id = usuario.Id,
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

            // Redirigir
            return RedirectToAction("GuardarSesion", "Sisco");

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
