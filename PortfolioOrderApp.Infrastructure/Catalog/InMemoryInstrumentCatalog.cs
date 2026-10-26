using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Infrastructure.Catalog;

public class InMemoryInstrumentCatalog : IInstrumentCatalog
{
    // Catalogo ejemplo, persiste durante la ejecucion
    private static readonly List<Instrument> _instruments = new()
    {
        new Instrument { Id = 1, Ticker = "AAPL", Name = "Apple Inc.", AssetType = "Accion", Price = 195.30m },
        new Instrument { Id = 2, Ticker = "TLT",  Name = "iShares 20+ Year Treasury Bond ETF", AssetType = "ETF", Price = 92.15m },
        new Instrument { Id = 3, Ticker = "VWO",  Name = "Vanguard FTSE Emerging Markets ETF", AssetType = "ETF", Price = 44.80m },
        new Instrument { Id = 4, Ticker = "BONO10Y", Name = "Bono soberano 10 anios", AssetType = "Bono", Price = 101.20m }
    };

    // Devuelve todos los instrumentos del catalogo.
    public IReadOnlyList<Instrument> GetInstruments() => _instruments;

    // Busca un instrumento por Id; devuelve null si no existe.
    public Instrument? GetById(int id) => _instruments.FirstOrDefault(i => i.Id == id);
}