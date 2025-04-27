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
    public IActionResult CalcularSatisfaccion(EncuestaSatisfaccion datos)
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

        // Guardar en la base de datos si el usuario está autenticado
        if (User.Identity.IsAuthenticated)
        {
            int? userId = UsuarioHelper.ObtenerIdUsuarioActual(User);

            var ultimaSesion = _context.Sesiones
                .Where(s => s.IdUsuario == userId && s.Puntuacion > 0)
                .OrderByDescending(s => s.FechaRealizacion)
                .FirstOrDefault();

            if (ultimaSesion != null)
            {
                int lastId = _context.RespuestasEncuesta.Max(r => (int?)r.Id) ?? 0;

                var respuestasBD = new List<RespuestaEncuesta>();

                var nivel = _context.NivelesSatisfaccion
                    .FirstOrDefault(n => n.Descripcion.ToLower() == interpretacion.ToLower());

                if (nivel != null)
                {
                    ultimaSesion.IdNivelSatisfaccion = nivel.Id;
                }

                var nivp = _context.NivelesSatisfaccion
                    .FirstOrDefault(n => n.Id == datos.Platform);
                respuestasBD.Add(new RespuestaEncuesta
                {
                    Id = ++lastId,
                    IdSesion = ultimaSesion.Id,
                    IdPreguntaEncuesta = 1,
                    ValorRespuesta = (byte)datos.Platform,
                    ValorResTexto = nivp.Descripcion,
                    FechaRespuesta = DateTime.Now
                });

                var nivt = _context.NivelesSatisfaccion
                    .FirstOrDefault(n => n.Id == datos.Test);
                respuestasBD.Add(new RespuestaEncuesta
                {
                    Id = ++lastId,
                    IdSesion = ultimaSesion.Id,
                    IdPreguntaEncuesta = 2,
                    ValorRespuesta = (byte)datos.Test,
                    ValorResTexto = nivt.Descripcion,
                    FechaRespuesta = DateTime.Now
                });

                var nivm = _context.NivelesSatisfaccion
                    .FirstOrDefault(n => n.Id == datos.Minigame);
                respuestasBD.Add(new RespuestaEncuesta
                {
                    Id = ++lastId,
                    IdSesion = ultimaSesion.Id,
                    IdPreguntaEncuesta = 3,
                    ValorRespuesta = (byte)datos.Minigame,
                    ValorResTexto = nivm.Descripcion,
                    FechaRespuesta = DateTime.Now
                });

                if (!string.IsNullOrWhiteSpace(datos.Comments))
                {
                    respuestasBD.Add(new RespuestaEncuesta
                    {
                        Id = ++lastId,
                        IdSesion = ultimaSesion.Id,
                        IdPreguntaEncuesta = 4, // Comentarios libres
                        ValorRespuesta = 0,
                        ValorResTexto = datos.Comments,
                        FechaRespuesta = DateTime.Now
                    });
                }

                _context.RespuestasEncuesta.AddRange(respuestasBD);
                _context.SaveChanges();
            }

        }

        // Redireccionar al Login
        return RedirectToAction("Index");

    }

}
