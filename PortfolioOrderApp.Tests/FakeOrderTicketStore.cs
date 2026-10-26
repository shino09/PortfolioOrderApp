using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioOrderApp.Tests.Fakes
{
    public class RecordingComplianceAudit : IComplianceAudit
    {
        // Entradas capturadas (Ticker, Lado, Cantidad).
        public List<(string Ticker, OrderSide Side, int Quantity)> Entries { get; } = new();

        // Registra la entrada en la lista en memoria en vez de escribir en disco.
        public void LogOrderConfirmation(string ticker, OrderSide side, int quantity)
            => Entries.Add((ticker, side, quantity));
    }
}