using System.Collections.Generic;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Application.Abstractions
{ 
public interface IInstrumentCatalog
{

    // Devuelve todos los instrumentos disponibles para operar.

    IReadOnlyList<Instrument> GetInstruments();

    // Busca un instrumento por su Id; devuelve null si no existe.
    Instrument? GetById(int id);
}
}