using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OK.Data;
using OK.Helpers;
using OK.Models;
using System.Security.Claims;

namespace OK.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Perfil()
        {            
            int userId = UsuarioHelper.ObtenerIdUsuarioActual(User);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == userId);

            if (usuario == null)
                return RedirectToAction("Login", "Login");

            var ultimaSesion = _context.Sesiones
                .Include(s => s.NivelEstres)
                .Include(s => s.Recomendacion)
                .Where(s => s.IdUsuario == userId)
                .OrderByDescending(s => s.FechaRealizacion)
                .FirstOrDefault();

            if (ultimaSesion != null)
            {
                ViewBag.Porcentaje = ultimaSesion.Puntuacion;
                ViewBag.Nivel = ultimaSesion.NivelEstres?.Descripcion ?? "Desconocido";
                ViewBag.Recomendacion = ultimaSesion.Recomendacion?.RecomendacionTexto ?? "Sin recomendaciones";
            }
            else
            {
                ViewBag.Porcentaje = 0;
                ViewBag.Nivel = "Sin datos";
                ViewBag.Recomendacion = "Aún no realizas ningún test.";
            }
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(Usuario model)
        {
            int userId = UsuarioHelper.ObtenerIdUsuarioActual(User);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == userId);

            if (usuario == null)
                return RedirectToAction("Login", "Login");

            // Actualizar campos modificables
            usuario.Nombre = model.Nombre;
            usuario.ApPat = model.ApPat;
            usuario.ApMat = model.ApMat;
            usuario.Edad = model.Edad;
            usuario.Carrera = model.Carrera;
            usuario.Semestre = model.Semestre;

            // Si se solicitó cambio de contraseña
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                usuario.Password = CifradoHelper.Hash(model.Password);
            }

            _context.SaveChanges();

            return RedirectToAction("Perfil");
        }
    }
}
