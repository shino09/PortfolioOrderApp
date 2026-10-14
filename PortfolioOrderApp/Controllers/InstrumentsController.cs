using Microsoft.AspNetCore.Mvc;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Web.Controllers
{
    public class InstrumentsController : Controller
    {
        private readonly IInstrumentCatalog _catalog;

        public InstrumentsController(IInstrumentCatalog catalog) => _catalog = catalog;

        // GET /Instruments: muestra la vista con la lista de instrumentos.
        public IActionResult Index()
        {
            return View(_catalog.GetInstruments());
        }
    }
}
