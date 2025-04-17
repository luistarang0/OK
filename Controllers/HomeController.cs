using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OK.Models;

namespace OK.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
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
