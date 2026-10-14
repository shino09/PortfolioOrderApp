using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortfolioOrderApp.Application.Services;

namespace PortfolioOrderApp.Web.Controllers;

public class DepositsController : Controller
{
    private readonly DepositRequestService _depositService;
  
    // Inyecta el servicio de depositos (DI).
    public DepositsController(DepositRequestService depositService) => _depositService = depositService;

    // GET /Deposits: lista los depositos solicitados.
    public IActionResult Index()
    {
        return View(_depositService.GetAll());
    }

    // POST /Deposits/Create: recibe monto + comprobante, valida y registra el deposito.

    [HttpPost]
    public IActionResult Create(decimal amount, IFormFile? receiptFile)
    {
        using var stream = receiptFile?.OpenReadStream();

        if (!_depositService.TryCreateDeposit(amount, stream, receiptFile?.FileName, out string? error))
        {
            ModelState.AddModelError("receiptFile", error);
            return View("Index", _depositService.GetAll());
        }

        return RedirectToAction("Index");
    }
}