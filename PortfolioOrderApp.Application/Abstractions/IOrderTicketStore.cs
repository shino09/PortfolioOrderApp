using System.Collections.Generic;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Application.Abstractions
{
    public interface IOrderTicketStore
    {
        // Devuelve las lineas de la orden en curso.
        List<OrderLine> GetDraft();

        // Agrega una linea a la orden en curso.
        void AddLine(OrderLine line);

        // Limpia la orden en curso (se usa al confirmar).
        void Clear();
    }
}