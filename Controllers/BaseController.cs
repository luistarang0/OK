using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OK.Data;
using OK.Helpers;

namespace OK.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool puedeRealizarTest = true;

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    int? userId = UsuarioHelper.ObtenerIdUsuarioActual(User);

                    var ultimaSesion = _context.Sesiones
                        .Where(s => s.IdUsuario == userId && s.Puntuacion > 0)
                        .OrderByDescending(s => s.FechaRealizacion)
                        .FirstOrDefault();

                    if (ultimaSesion != null)
                    {
                        var dias = (DateTime.Now - ultimaSesion.FechaRealizacion).TotalDays;
                        puedeRealizarTest = dias >= 30;
                    }
                }
                catch
                {
                    puedeRealizarTest = true; // fallback seguro
                }
            }

            ViewData["PuedeRealizarTest"] = puedeRealizarTest;

            base.OnActionExecuting(context);
        }
    }
}
