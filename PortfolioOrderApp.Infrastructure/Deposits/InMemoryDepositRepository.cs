using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Infrastructure.Deposits;

public class InMemoryDepositRepository : IDepositRepository
{
    private readonly List<DepositRequest> _deposits = new();
    private int _nextId = 1;

    // Devuelve los depositos ordenados por fecha descendente (los mas recientes primero).
    public IReadOnlyList<DepositRequest> GetAll()
        => _deposits.OrderByDescending(d => d.RequestedAtUtc).ToList();

    // Asigna un Id correlativo y agrega el deposito a la lista.
    public DepositRequest Add(DepositRequest deposit)
    {
        deposit.Id = _nextId++;
        _deposits.Add(deposit);
        return deposit;
    }
}