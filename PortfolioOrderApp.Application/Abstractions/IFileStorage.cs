using System.Collections.Generic;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Application.Abstractions
{
    public interface IFileStorage
    {
        // Guarda el contenido del stream en el archivo indicado.
        string Save(Stream fileStream, string fileName);   // Devuelve el nombre/ruta del archivo guardado
    }
}