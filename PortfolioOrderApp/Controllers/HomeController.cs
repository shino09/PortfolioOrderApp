using Microsoft.AspNetCore.Mvc;
using PortfolioOrderApp.Web.Models;
using System.Diagnostics;

namespace PruebaPlanVital1.Controllers
{
    public class HomeController : Controller
    {


        private readonly ILogger<HomeController> _logger;

        // Inyecta el logger de la aplicacion.
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        // GET /Home/Error: pagina de error; la usa UseExceptionHandler para mostrar la falla.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
