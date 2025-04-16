using Microsoft.AspNetCore.Mvc;
using OK.Models;

namespace OK.Controllers
{
    public class SiscoController : Controller
    {
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

            return View();
        }

    }
}
