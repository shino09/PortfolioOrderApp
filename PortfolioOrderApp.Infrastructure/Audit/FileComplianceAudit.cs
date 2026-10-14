using Microsoft.AspNetCore.Hosting;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Infrastructure.Audit;

public class FileComplianceAudit : IComplianceAudit
{
    private readonly IWebHostEnvironment _env;

    // Recibe el entorno web para resolver la carpeta App_Data dentro del ContentRoot.
    public FileComplianceAudit(IWebHostEnvironment env) => _env = env;

    // Crea App_Data/compliance_audit.log y agrega una linea por orden confirmada.
    public void LogOrderConfirmation(string ticker, OrderSide side, int quantity)
    {
        var directory = Path.Combine(_env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);

        var logPath = Path.Combine(directory, "compliance_audit.log");
        var line = $"{DateTime.UtcNow:o} - Orden confirmada: {side} {ticker} x{quantity}{Environment.NewLine}";
        File.AppendAllText(logPath, line);
    }
}