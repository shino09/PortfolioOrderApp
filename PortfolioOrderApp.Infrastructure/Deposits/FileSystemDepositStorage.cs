using Microsoft.AspNetCore.Hosting;
using PortfolioOrderApp.Application.Abstractions;

namespace PortfolioOrderApp.Infrastructure.Deposits;

public class FileSystemDepositStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    // Recibe el entorno web para resolver la ruta raiz de wwwroot.
    public FileSystemDepositStorage(IWebHostEnvironment env) => _env = env;

    // Crea wwwroot/uploads/deposits si no existe y copia el stream al archivo indicado.
    public string Save(Stream fileStream, string fileName)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "deposits");
        Directory.CreateDirectory(uploadsPath);

        var baseName = Path.GetFileNameWithoutExtension(fileName);
        foreach (var existing in Directory.GetFiles(uploadsPath, baseName + ".*"))
            File.Delete(existing);

        var physicalFilePath = Path.Combine(uploadsPath, fileName);
        using var file = File.Create(physicalFilePath);
        fileStream.CopyTo(file);

        return fileName;
    }
}