using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OK.Data;
using OK.Helpers;
using OK.Models;

namespace OK.Controllers
{
    public class SiscoController : Controller
    {

        private readonly ApplicationDbContext _context;

        public SiscoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Instrucciones()
        {
            return View();
        }
        public IActionResult Comienzo()
        {
            return View();
        }

        public IActionResult Sisco()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Flujo(Respuestas respuestas)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Respuestas"] = JsonConvert.SerializeObject(respuestas);
                TempData.Keep("Respuestas");
                TempData["RedirigirDespuesDeLogin"] = "DeserializarResultados";
                return RedirectToAction("Login", "Login");
            }

            return CalcularResultados(respuestas);
        }

        public IActionResult DeserializarResultados()
        {
            if (!TempData.ContainsKey("Respuestas"))
                return RedirectToAction("Index", "Home");

            var respuestas = JsonConvert.DeserializeObject<Respuestas>(TempData["Respuestas"].ToString());

            return CalcularResultados(respuestas);
        }

        [HttpGet]
        public IActionResult Resultados(int idSesion)
        {
            var sesion = _context.Sesiones
                    .Include(s => s.NivelEstres)
                    .Include(s => s.Recomendacion)
                    .FirstOrDefault(s => s.Id == idSesion);

            if (sesion == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Porcentaje = sesion.Puntuacion;
            ViewBag.Nivel = sesion.NivelEstres?.Descripcion ?? "Desconocido";
            ViewBag.Tipo = sesion.Recomendacion?.Tipo ?? "No clasificado";

            return View();
        }

        public IActionResult CalcularResultados(Respuestas respuestas)
        {
            var resultados = respuestas.Estresores
            .Concat(respuestas.Sintomas)
            .Concat(respuestas.Afrontamiento)
            .ToList();

            double media = resultados.Average();
            double porcentaje = media * 20;

            string nivelNombre = porcentaje <= 48 ? "Leve" :
                                 porcentaje <= 60 ? "Moderado" : "Severo";

            string tipo = porcentaje <= 48 ? "Físico" :
                          porcentaje <= 60 ? "Conductual" : "Cognitivo";

            // Obtener entidades desde la BD
            var nivel = _context.NivelesEstres.FirstOrDefault(n => n.Descripcion == nivelNombre);
            var recomendacion = _context.Recomendaciones.FirstOrDefault(r => r.Tipo.ToLower() == tipo.ToLower());

            int lastId = _context.Sesiones.Max(s => (int?)s.Id) ?? 0;
            
            var sesion = new SesionEvaluaciones
            {
                FechaRealizacion = DateTime.Now,
                IdUsuario = UsuarioHelper.ObtenerIdUsuarioActual(User),
                Puntuacion = (byte)porcentaje,
                IdNivelEstres = nivel?.Id,
                IdRecomendacion = recomendacion?.Id,
                Estatus = 1
            };

            sesion.Id = lastId + 1;

            _context.Sesiones.Add(sesion);
            _context.SaveChanges();

            var preguntas = new List<int>();
            preguntas.AddRange(respuestas.Estresores);
            preguntas.AddRange(respuestas.Sintomas);
            preguntas.AddRange(respuestas.Afrontamiento);

            lastId = _context.RespuestasTest.Max(r => (int?)r.Id) ?? 0;
            for (int i = 0; i < preguntas.Count; i++)
            {
                lastId++;
                _context.RespuestasTest.Add(new RespuestaTest
                {
                    Id = lastId,
                    IdSesion = sesion.Id,
                    IdPreguntaTest = i + 1,
                    ValorRespuesta = (byte)preguntas[i],
                    ValorResTexto = preguntas[i].ToString()
                });
            }

            _context.SaveChanges();

            return RedirectToAction("Resultados", "Sisco", new { idSesion = sesion.Id });
        }             
    }
}
