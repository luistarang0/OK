using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OK.Data;
using OK.Models;

namespace OK.Controllers
{
    public class SiscoController : Controller
    {

        private readonly ApplicationDbContext _context;


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
        public IActionResult Resultados(Respuestas respuestas)
        {
            var resultados = respuestas.Estresores
            .Concat(respuestas.Sintomas)
            .Concat(respuestas.Afrontamiento)
            .ToList();

            double media = resultados.Average();
            double porcentaje = media * 20;

            string nivel;
            if (porcentaje <= 48) nivel = "Leve";
            else if (porcentaje <= 60) nivel = "Moderado";
            else nivel = "Severo";

            string tipo;
            if (porcentaje <= 48) tipo = "Físico";
            else if (porcentaje <= 60) tipo = "Conductual";
            else tipo = "Cognitivo";

            ViewBag.Media = Math.Round(media, 2);
            ViewBag.Porcentaje = Math.Round(porcentaje, 2);
            ViewBag.Nivel = nivel;
            ViewBag.Tipo = tipo;

            TempData["ResultadosTest"] = JsonConvert.SerializeObject(new
            {
                Media = media,
                Porcentaje = porcentaje,
                Nivel = nivel,
                Tipo = tipo,
                Estresores = respuestas.Estresores,
                Sintomas = respuestas.Sintomas,
                Afrontamiento = respuestas.Afrontamiento
            });

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Login"); // o como sea tu controlador de autenticación
            }

            return View();
        }

        [HttpPost]
        public IActionResult GuardarSesion()
        {
            if (!TempData.ContainsKey("ResultadosTest")) return RedirectToAction("Index", "Home");

            var json = TempData["ResultadosTest"].ToString();
            var data = JsonConvert.DeserializeObject<ResultadosTemp>(json);


            // Ejemplo: buscar nivel y recomendación según porcentaje/tipo
            var nivelEstres = _context.NivelesEstres
                .FirstOrDefault(n => (int)data.Porcentaje <= n.Rango);
            var recomendacion = _context.Recomendaciones
                .FirstOrDefault(r => r.Tipo.ToLower() == ((string)data.Tipo).ToLower());

            var sesion = new SesionEvaluaciones
            {
                FechaRealizacion = DateTime.Now,
                IdUsuario = ObtenerIdUsuarioActual(), // método que toma el ID del User actual
                Puntuacion = (int)data.Porcentaje,
                IdNivelEstres = nivelEstres?.Id,
                IdRecomendacion = recomendacion?.Id,
                IdNivelSatisfaccion = null, // si aplica después
                Estatus = 1 // o 2 según la lógica
            };

            _context.Sesiones.Add(sesion);
            _context.SaveChanges();

            // Insertar respuestas individuales en Respuesta_test
            var preguntas = new List<int>();
            preguntas.AddRange(data.Estresores);
            preguntas.AddRange(data.Sintomas);
            preguntas.AddRange(data.Afrontamiento);


            for (int i = 0; i < preguntas.Count; i++)
            {
                _context.RespuestasTest.Add(new RespuestaTest
                {
                    IdSesion = sesion.Id,
                    IdPreguntaTest = i + 1,
                    ValorRespuesta = preguntas[i],
                    ValorResTexto = preguntas[i].ToString()
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Resultados", "Sisco");
        }

        private int ObtenerIdUsuarioActual()
        {
            var userIdClaim = User.FindFirst("Id");

            if (userIdClaim == null)
                throw new Exception("No se encontró el ID del usuario en los claims.");

            return int.Parse(userIdClaim.Value);
        }

    }
}
