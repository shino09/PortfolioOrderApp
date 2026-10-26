namespace PortfolioOrderApp.Domain
{
    public class Instrument
    {
        public int Id { get; set; }
        public string Ticker { get; set; } = string.Empty; // Codigo AAPL,etc.
        public string Name { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty; // "Accion", "Bono", "ETF"
        public decimal Price { get; set; }
    }
}
