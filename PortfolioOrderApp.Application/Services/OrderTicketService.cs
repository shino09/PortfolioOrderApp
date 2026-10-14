using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Application.Services;

// Caso de uso de ordenes: maneja el borrador de la orden y su confirmacion.
public class OrderTicketService
{
    private readonly IOrderTicketStore _store;
    private readonly IComplianceAudit _audit;

    // Inyecta el almacen de la orden en curso y el auditor de cumplimiento.
    public OrderTicketService(IOrderTicketStore store, IComplianceAudit audit)
    {
        _store = store;
        _audit = audit;
    }

    // Devuelve las lineas de la orden en curso.
    public List<OrderLine> GetDraft() => _store.GetDraft();

    // Agrega una linea al borrador de la orden.
    public void AddLine(OrderLine line) => _store.AddLine(line);


    // Calcula el total estimado (cantidad x precio) cruzando el catalogo; ignora instrumentos desconocidos.
    public decimal GetEstimatedTotal(IEnumerable<Instrument> catalog)
    {
        return _store.GetDraft().Sum(l =>
        {
            var instrument = catalog.FirstOrDefault(i => i.Id == l.InstrumentId);
            return instrument is null ? 0 : l.Quantity * instrument.Price;
        });
    }

    // Confirma la orden: registra una entrada de auditoria por linea y luego limpia el borrador.
    //queda en App_Data/compliance_audit.log
    public void ConfirmTicket(IEnumerable<Instrument> catalog)
    {
        foreach (var line in _store.GetDraft())
        {
            var instrument = catalog.FirstOrDefault(i => i.Id == line.InstrumentId);
            _audit.LogOrderConfirmation(instrument?.Ticker ?? "N/A", line.Side, line.Quantity);
        }

        _store.Clear();
    }
}