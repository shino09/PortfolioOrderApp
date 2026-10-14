using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Application.Services;
using PortfolioOrderApp.Infrastructure.Audit;
using PortfolioOrderApp.Infrastructure.Catalog;
using PortfolioOrderApp.Infrastructure.Deposits;
using PortfolioOrderApp.Infrastructure.Session;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Acceso a HttpContext (lo usa SessionOrderTicketStore)
builder.Services.AddHttpContextAccessor();

// Sesión en memoria (almacena la orden en curso por usuario)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30));

// Infrastructure — implementaciones (Singleton para lo que persiste en memoria)
builder.Services.AddSingleton<IInstrumentCatalog, InMemoryInstrumentCatalog>();
builder.Services.AddSingleton<IDepositRepository, InMemoryDepositRepository>();
builder.Services.AddSingleton<IFileStorage, FileSystemDepositStorage>();
builder.Services.AddSingleton<IComplianceAudit, FileComplianceAudit>();

// Sesión por request → Scoped
builder.Services.AddScoped<IOrderTicketStore, SessionOrderTicketStore>();

// Application — casos de uso
builder.Services.AddScoped<OrderTicketService>();
builder.Services.AddScoped<DepositRequestService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Trazabilidad: mide cuánto tarda cada request (reemplaza a ComplianceTraceModule)
app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    Console.WriteLine($"{context.Request.Path} tomo {stopwatch.ElapsedMilliseconds} ms");
});

app.UseStaticFiles(); // Sirve los archivos de wwwroot.

app.UseRouting(); // Habilita el enrutamiento.

app.UseSession(); // Activa la sesion (necesario antes de usarla en los stores/controllers).

app.UseAuthorization(); // Autorizacion (no se usa autenticacion en esta prueba).

// Ruta por defecto: al entrar a "/" se muestra el catalogo de instrumentos.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instruments}/{action=Index}/{id?}");

app.Run();