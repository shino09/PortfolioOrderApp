using System;

namespace PortfolioOrderApp.Domain
{
    public class DepositRequest
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string ReceiptFileName { get; set; } = string.Empty; // Nombre de archivo comprobante
        public DateTime RequestedAtUtc { get; set; } // Fecha solicitud
        public string Status { get; set; } = string.Empty;         // Estado "Pendiente de validacion"
    }
}
