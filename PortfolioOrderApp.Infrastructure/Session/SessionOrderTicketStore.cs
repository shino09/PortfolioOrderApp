using Microsoft.AspNetCore.Http;
using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;
using System.Text.Json;

namespace PortfolioOrderApp.Infrastructure.Session;

public class SessionOrderTicketStore : IOrderTicketStore
{
    private const string SessionKey = "CurrentOrderTicket";
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Recibe el accesor de HttpContext para llegar a la sesion del request actual.
    public SessionOrderTicketStore(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ISession Session => _httpContextAccessor.HttpContext?.Session!;

    // Lee la orden en curso desde la sesion (JSON); si no existe, devuelve una lista vacia.
    public List<OrderLine> GetDraft()
    {
        var json = Session.GetString(SessionKey);
        if (json is null)
        {
            var draft = new List<OrderLine>();
            Session.SetString(SessionKey, JsonSerializer.Serialize(draft));
            return draft;
        }
        return JsonSerializer.Deserialize<List<OrderLine>>(json) ?? new List<OrderLine>();
    }

    // Agrega una linea a la orden en curso y la vuelve a serializar en la sesion.
    public void AddLine(OrderLine line)
    {
        var draft = GetDraft();
        draft.Add(line);
        Session.SetString(SessionKey, JsonSerializer.Serialize(draft));
    }

    // Elimina la orden en curso de la sesion.
    public void Clear() => Session.Remove(SessionKey);
}