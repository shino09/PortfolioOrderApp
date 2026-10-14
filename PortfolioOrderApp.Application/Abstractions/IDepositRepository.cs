using System.Collections.Generic;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Application.Abstractions
{
    public interface IDepositRepository
    {
        // Devuelve todos los depositos registrados.
        IReadOnlyList<DepositRequest> GetAll();

        // Agrega un nuevo deposito al repositorio.
        DepositRequest Add(DepositRequest deposit);   
    }
}