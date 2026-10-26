using Microsoft.AspNetCore.Mvc;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Application.Services;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Web.Controllers;

public class OrderTicketController : Controller
{
    private readonly OrderTicketService _ticketService;
    private readonly IInstrumentCatalog _catalog;

    // Inyecta el servicio de ordenes y el catalogo (para calcular el total estimado).
    public OrderTicketController(OrderTicketService ticketService, IInstrumentCatalog catalog)
    {
        _ticketService = ticketService;
        _catalog = catalog;
    }

    // GET /OrderTicket: muestra las lineas de la orden en curso y el total estimado.
    public IActionResult Index()
    {
        var draft = _ticketService.GetDraft();
        ViewBag.EstimatedTotal = _ticketService.GetEstimatedTotal(_catalog.GetInstruments());
        return View(draft);
    }

    // POST /OrderTicket/Add: agrega una linea (instrumento, lado, cantidad) a la orden en curso.

    [HttpPost]
    public IActionResult Add(int instrumentId, OrderSide side, int quantity, string ticker)
    {
        _ticketService.AddLine(new OrderLine
        {
            InstrumentId = instrumentId,
            Side = side,
            Quantity = quantity,
            Ticker = ticker
        });
        return RedirectToAction("Index");
    }

    // POST /OrderTicket/Confirm: confirma la orden (genera auditoria) y limpia la orden en curso.

    [HttpPost]
    public IActionResult Confirm()
    {
        _ticketService.ConfirmTicket(_catalog.GetInstruments());
        return RedirectToAction("Index");
    }
}