Tengo una aplicación ASP.NET MVC 5 sobre .NET Framework 4.7 con esta arquitectura:

Un proyecto MVC con Controllers, Views (Razor) y Models.
Entidades del dominio: Instrument, OrderLine, DepositRequest y el enum OrderSide.
Servicios de negocio (OrderTicketService, DepositRequestService) instanciados directamente con new dentro de los controllers, sin inyección de dependencias.
Persistencia en memoria (carga seed), sin base de datos, y además estado compartido en variables static.
La orden en curso se guarda por sesión de usuario con HttpContext.Current.Session.
Subida de comprobantes con HttpPostedFileBase y Server.MapPath, guardados en una carpeta pública (uploads/deposits).
Auditoría escrita a un archivo en disco al confirmar la orden.
Trazabilidad del tiempo de cada request con un módulo HTTP (ComplianceTraceModule).
Configuración en Web.config y arranque en Global.asax.
Deseo migrarla a .NET 8. ¿Qué arquitectura me conviene usar? 


Te conviene una arquitectura en capas (Clean Architecture liviana) con 4 proyectos + tests. El original tiene todo mezclado en el proyecto MVC (controllers + lógica + acceso a datos + HttpContext), así que separás por responsabilidad y desacoplás la lógica de ASP.NET.

Proyectos en .NET 8:

Domain: las entidades puras (Instrument, OrderLine, OrderSide, DepositRequest). Sin dependencias.
Application: los servicios y sus contratos (OrderTicketService, DepositRequestService, IOrderTicketStore, IInstrumentCatalog, IComplianceAudit, IFileStorage, IDepositRepository). No conoce ASP.NET ni HttpContext.
Infrastructure: las implementaciones reales (sesión, catálogo en memoria, repositorio de depósitos, guardado de comprobantes, auditoría a archivo).
Web: los controllers, las vistas Razor, la inyección de dependencias y el pipeline (Program.cs).
Tests: las pruebas unitarias de la lógica.
Dirección de dependencias: Web e Infrastructure → Application → Domain. Nunca al revés.

PortfolioOrderApp.sln
PortfolioOrderApp.Domain/          (sin referencias)
PortfolioOrderApp.Application/     → Domain
PortfolioOrderApp.Infrastructure/  → Application, Domain
PortfolioOrderApp.Web/             → Application, Infrastructure
PortfolioOrderApp.Tests/           → Application
PortfolioOrderApp.Application.csproj:

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\PortfolioOrderApp.Domain\PortfolioOrderApp.Domain.csproj" />
  </ItemGroup>
</Project>
Cómo se mapea el original a la nueva arquitectura:


Models / entidades → Domain
Services con new en controllers → Application (+ interfaces)
HttpContext.Current.Session → Infrastructure (SessionOrderTicketStore)
Server.MapPath + HttpPostedFileBase → Infrastructure (IFileStorage / IWebHostEnvironment)
Auditoría a archivo → Infrastructure (IComplianceAudit)
ComplianceTraceModule → Middleware en Web
Web.config / Global.asax → appsettings.json / Program.cs
Controllers + Views → Web
Por qué esta: resuelve los problemas del original (servicios con new, estado static, dependencia de HttpContext) y deja la lógica testeable sin levantar el servidor, sin sobre-dimensionar una app chica. Podés usar MVC sobre Core (menor riesgo, reutilizás las vistas) sin que la arquitectura dependa de eso.



¿Cuál me conviene más: MVC sobre Core, Razor Pages o Minimal API?

El original ya está construido con controllers y vistas Razor, así que el port es casi directo y reutilizás las vistas tal cual. Razor Pages serviría para pantallas aisladas y Minimal API para endpoints, pero en este alcance implicarían reescribir sin un beneficio claro.

MVC sobre Core: menor riesgo, reutiliza controllers y vistas, mismo modelo mental del equipo.
Razor Pages: bueno para páginas independientes, pero acá obliga a reorganizar la lógica por página.
Minimal API: liviano para endpoints, pero no encaja con vistas Razor ni con el flujo de formularios actual.
Por eso conviene MVC sobre Core. Igual justificá la elección en las notas de migración.




Dado lo que tengo en .NET Framework, ¿cómo me conviene dejar la infraestructura en la nueva arquitectura?
PortfolioOrderApp.Infrastructure.csproj:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\PortfolioOrderApp.Application\PortfolioOrderApp.Application.csproj" />
    <ProjectReference Include="..\PortfolioOrderApp.Domain\PortfolioOrderApp.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```
Sesión (reemplaza HttpContext.Current.Session):
```csharp
using Microsoft.AspNetCore.Http;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;
using System.Text.Json;

namespace PortfolioOrderApp.Infrastructure.Session;

public class SessionOrderTicketStore : IOrderTicketStore
{
    private const string SessionKey = "CurrentOrderTicket";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionOrderTicketStore(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ISession Session => _httpContextAccessor.HttpContext?.Session!;

    public List<OrderLine> GetDraft()
    {
        var json = Session.GetString(SessionKey);
        if (json is null)
        {
            var draft = new List<OrderLine>();
            Session.SetString(SessionKey, JsonSerializer.Serialize(draft));
            return draft;
        }
        return JsonSerializer.Deserialize<List<OrderLine>>(json) ?? new List<OrderLine>();
    }

    public void AddLine(OrderLine line)
    {
        var draft = GetDraft();
        draft.Add(line);
        Session.SetString(SessionKey, JsonSerializer.Serialize(draft));
    }

    public void Clear() => Session.Remove(SessionKey);
}
```
Catálogo:
```csharp
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Infrastructure.Catalog;

public class InMemoryInstrumentCatalog : IInstrumentCatalog
{
    private static readonly List<Instrument> _instruments = new()
    {
        new Instrument { Id = 1, Ticker = "AAPL", Name = "Apple Inc.", AssetType = "Accion", Price = 195.30m },
        new Instrument { Id = 2, Ticker = "TLT",  Name = "iShares 20+ Year Treasury Bond ETF", AssetType = "ETF", Price = 92.15m },
        new Instrument { Id = 3, Ticker = "VWO",  Name = "Vanguard FTSE Emerging Markets ETF", AssetType = "ETF", Price = 44.80m },
        new Instrument { Id = 4, Ticker = "BONO10Y", Name = "Bono soberano 10 anios", AssetType = "Bono", Price = 101.20m }
    };

    public IReadOnlyList<Instrument> GetInstruments() => _instruments;
    public Instrument? GetById(int id) => _instruments.FirstOrDefault(i => i.Id == id);
}
```
Depósitos y comprobantes (reemplaza Server.MapPath):
```csharp
using Microsoft.AspNetCore.Hosting;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Infrastructure.Deposits;

public class FileSystemDepositStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;
    public FileSystemDepositStorage(IWebHostEnvironment env) => _env = env;

    public string Save(Stream fileStream, string fileName)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "deposits");
        Directory.CreateDirectory(uploadsPath);
        var physicalFilePath = Path.Combine(uploadsPath, fileName);
        using var file = File.Create(physicalFilePath);
        fileStream.CopyTo(file);
        return fileName;
    }
}
```
Auditoría:
```csharp
using Microsoft.AspNetCore.Hosting;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Infrastructure.Audit;

public class FileComplianceAudit : IComplianceAudit
{
    private readonly IWebHostEnvironment _env;
    public FileComplianceAudit(IWebHostEnvironment env) => _env = env;

    public void LogOrderConfirmation(string ticker, OrderSide side, int quantity)
    {
        var directory = Path.Combine(_env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        var logPath = Path.Combine(directory, "compliance_audit.log");
        var line = $"{DateTime.UtcNow:o} - Orden confirmada: {side} {ticker} x{quantity}{Environment.NewLine}";
        File.AppendAllText(logPath, line);
    }
}
```


¿Cómo dejo los servicios y las interfaces en Application?
Interfaces (Abstractions):
```csharp
public interface IInstrumentCatalog
{
    IReadOnlyList<Instrument> GetInstruments();
    Instrument? GetById(int id);
}

public interface IOrderTicketStore
{
    List<OrderLine> GetDraft();
    void AddLine(OrderLine line);
    void Clear();
}

public interface IComplianceAudit
{
    void LogOrderConfirmation(string ticker, OrderSide side, int quantity);
}

public interface IFileStorage
{
    string Save(Stream fileStream, string fileName);
}

public interface IDepositRepository
{
    IReadOnlyList<DepositRequest> GetAll();
    DepositRequest Add(DepositRequest deposit);
}
```
Servicios (Services):
```csharp
public class OrderTicketService
{
    private readonly IOrderTicketStore _store;
    private readonly IComplianceAudit _audit;

    public OrderTicketService(IOrderTicketStore store, IComplianceAudit audit)
    {
        _store = store;
        _audit = audit;
    }

    public List<OrderLine> GetDraft() => _store.GetDraft();
    public void AddLine(OrderLine line) => _store.AddLine(line);

    public decimal GetEstimatedTotal(IEnumerable<Instrument> catalog)
        => _store.GetDraft().Sum(l =>
        {
            var instrument = catalog.FirstOrDefault(i => i.Id == l.InstrumentId);
            return instrument is null ? 0 : l.Quantity * instrument.Price;
        });

    public void ConfirmTicket(IEnumerable<Instrument> catalog)
    {
        foreach (var line in _store.GetDraft())
        {
            var instrument = catalog.FirstOrDefault(i => i.Id == line.InstrumentId);
            _audit.LogOrderConfirmation(instrument?.Ticker ?? "N/A", line.Side, line.Quantity);
        }
        _store.Clear();
    }
}
```


¿Dónde se debería manejar la sesión?
SessionOrderTicketStore (ver pregunta 4) y el registro en DI:
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30));
builder.Services.AddScoped<IOrderTicketStore, SessionOrderTicketStore>();
...
app.UseSession();
```


¿Cómo migro la subida del comprobante y el guardado de archivos a .NET 8?
Controller:
```csharp
public class DepositsController : Controller
{
    private readonly DepositRequestService _depositService;
    public DepositsController(DepositRequestService depositService) => _depositService = depositService;

    public IActionResult Index() => View(_depositService.GetAll());

    [HttpPost]
    [ValidateAntiForgeryToken]
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
```
Vista:
```cshtml
<form asp-action="Create" asp-controller="Deposits" method="post" enctype="multipart/form-data">
    <input name="amount" />
    <input type="file" name="receiptFile" />
    <button type="submit">Enviar solicitud</button>
</form>
```


¿Dónde escribo la auditoría al confirmar?
A un archivo en una carpeta persistente (no en bin ni en una temporal), con fecha ISO (DateTime.UtcNow.ToString("o")) y escritura segura. Formato libre, pero consultable. Registrá cada orden confirmada según pide el enunciado.


¿Cómo protejo los POST?
El FormTagHelper genera el token con asp-action, y la acción lo valida:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Confirm()
{
    _ticketService.ConfirmTicket(_catalog.GetInstruments());
    return RedirectToAction("Index");
}
```


¿Cómo mido el tiempo de cada request?
```csharp
app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    app.Logger.LogInformation("{Path} tomo {Ms} ms", context.Request.Path, stopwatch.ElapsedMilliseconds);
});
```


Me da el error: CS0246: no se encuentra el tipo o el espacio de nombres 'IWebHostEnvironment'.
```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```
```csharp
using Microsoft.AspNetCore.Hosting;
```


Me da el error: Cannot consume scoped service 'IHttpContextAccessor' from singleton 'IOrderTicketStore'.
```csharp
builder.Services.AddScoped<IOrderTicketStore, SessionOrderTicketStore>();
```


Me da el error: InvalidOperationException: Session has not been configured.
```csharp
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(...);
```

Me da el error: CS0246: no se encuentra el tipo 'RecordingComplianceAudit'.
Falta el using del namespace donde está la clase. Los dobles viven en PortfolioOrderApp.Tests.Fakes, así que agregá using PortfolioOrderApp.Tests.Fakes;. Verificá también que el namespace del archivo coincida con la carpeta.

¿Dime qué test xUnit debería hacer para probar?
```csharp
public class OrderTicketServiceTests
{
    private static readonly List<Instrument> Catalog = new()
    {
        new Instrument { Id = 1, Ticker = "AAPL", Price = 100m },
        new Instrument { Id = 2, Ticker = "TLT",  Price = 50m }
    };

    [Fact]
    public void AddLine_AgregaLaLineaAlBorrador()
    {
        var store = new FakeOrderTicketStore();
        var service = new OrderTicketService(store, new RecordingComplianceAudit());

        service.AddLine(new OrderLine { InstrumentId = 1, Ticker = "AAPL", Side = OrderSide.Compra, Quantity = 3 });

        Assert.Single(store.GetDraft());
    }

    [Fact]
    public void GetEstimatedTotal_SumaCantidadPorPrecio()
    {
        var service = new OrderTicketService(new FakeOrderTicketStore(), new RecordingComplianceAudit());
        service.AddLine(new OrderLine { InstrumentId = 1, Side = OrderSide.Compra, Quantity = 2 });
        service.AddLine(new OrderLine { InstrumentId = 2, Side = OrderSide.Venta, Quantity = 3 });

        Assert.Equal(350m, service.GetEstimatedTotal(Catalog));
    }

    [Fact]
    public void ConfirmTicket_RegistraUnaAuditoriaYLimpiaElBorrador()
    {
        var store = new FakeOrderTicketStore();
        var audit = new RecordingComplianceAudit();
        var service = new OrderTicketService(store, audit);
        service.AddLine(new OrderLine { InstrumentId = 1, Side = OrderSide.Compra, Quantity = 2 });

        service.ConfirmTicket(Catalog);

        Assert.Single(audit.Entries);
        Assert.Empty(store.GetDraft());
    }
}
```

¿Qué añado al Program.cs?
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30));

builder.Services.AddSingleton<IInstrumentCatalog, InMemoryInstrumentCatalog>();
builder.Services.AddSingleton<IDepositRepository, InMemoryDepositRepository>();
builder.Services.AddSingleton<IFileStorage, FileSystemDepositStorage>();
builder.Services.AddSingleton<IComplianceAudit, FileComplianceAudit>();

builder.Services.AddScoped<IOrderTicketStore, SessionOrderTicketStore>();
builder.Services.AddScoped<OrderTicketService>();
builder.Services.AddScoped<DepositRequestService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    app.Logger.LogInformation("{Path} tomo {Ms} ms", context.Request.Path, stopwatch.ElapsedMilliseconds);
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instruments}/{action=Index}/{id?}");

app.Run();
```

¿Qué últimos detalles debería considerar antes de subir a Git?
```gitignore
## .NET / Visual Studio
[Bb]in/
[Oo]bj/
*.user
*.suo
*.userosscache
*.sln.docstates
.vs/

## Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
publish/

## Rider / VS Code
.idea/
.vscode/

## Test results
[Tt]est[Rr]esult*/
*.trx
coverage*.xml
coverage*.json
*.coverage
*.coveragexml

## Datos generados en runtime (no versionar)
PortfolioOrderApp/wwwroot/uploads/
PortfolioOrderApp/App_Data/
*.log

## NuGet
*.nupkg
packages/
```
Sacá bin/ y obj/, no subas uploads ni secretos, y creá el .sln para compilar los 5 proyectos juntos.