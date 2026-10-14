using System.Collections.Generic;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Application.Abstractions
{
    public interface IComplianceAudit
    {
        // Registra una entrada de auditoria por una orden confirmada.
        void LogOrderConfirmation(string ticker, OrderSide side, int quantity);
    }
}