using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OK.Helpers;
using OK.Models;
using OK.Data;

namespace OK.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) : base(context)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        bool mostrarEncuesta = false;

        if (User.Identity.IsAuthenticated)
        {
            int? userId = UsuarioHelper.ObtenerIdUsuarioActual(User);

            var ultimaSesion = _context.Sesiones
                .Where(s => s.IdUsuario == userId && s.Puntuacion > 0)
                .OrderByDescending(s => s.FechaRealizacion)
                .FirstOrDefault();

            if (ultimaSesion != null)
            {
                // Buscar si ya existen respuestas de encuesta para esta sesión
                var yaRespondioEncuesta = _context.RespuestasEncuesta
                    .Any(r => r.IdSesion == ultimaSesion.Id);

                if (!yaRespondioEncuesta)
                {
                    mostrarEncuesta = true;
                }
            }
        }

        ViewBag.MostrarEncuesta = mostrarEncuesta;
        return View();
    }

    [HttpPost]
    public IActionResult CalcularSatisfaccion([FromBody] EncuestaSatisfaccion datos)
    {
        var respuestas = new List<int> { datos.Platform, datos.Test, datos.Minigame };

        double promedio = respuestas.Average(); // PS: 1 a 5
        double porcentaje = ((promedio - 1) / 4) * 100; // Normalización 0–100

        string interpretacion;
        if (porcentaje <= 20) interpretacion = "Muy insatisfecho";
        else if (porcentaje <= 40) interpretacion = "Insatisfecho";
        else if (porcentaje <= 60) interpretacion = "Neutral";
        else if (porcentaje <= 80) interpretacion = "Satisfecho";
        else interpretacion = "Muy satisfecho";

        // Aquí podrías guardar en la base de datos también...

        return Json(new
        {
            porcentaje = Math.Round(porcentaje, 2),
            interpretacion,
            comentario = datos.Comments
        });
    }

}
