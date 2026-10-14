using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Tests.Fakes
{
    public class FakeOrderTicketStore : IOrderTicketStore
    {
        // Devuelve el borrador en memoria.
        private readonly List<OrderLine> _lines = new();

        public List<OrderLine> GetDraft() => _lines;
        // Agrega una linea al borrador.
        public void AddLine(OrderLine line) => _lines.Add(line);
        
        // Vacia el borrador.
        public void Clear() => _lines.Clear();
    }
}