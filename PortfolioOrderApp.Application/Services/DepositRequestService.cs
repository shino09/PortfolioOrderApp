using PortfolioOrderApp.Application.Abstractions;
using PortfolioOrderApp.Domain;

namespace PortfolioOrderApp.Application.Services;

// Caso de uso de depositos: valida la solicitud, guarda el comprobante y registra el deposito carpeta wwwroot\uploads\deposits.
public class DepositRequestService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    // Tamano maximo permitido para el comprobante (5 MB).

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IDepositRepository _repository;
    private readonly IFileStorage _fileStorage;

    // Inyecta el repositorio de depositos y el almacenamiento de archivos.



    public DepositRequestService(IDepositRepository repository, IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public IReadOnlyList<DepositRequest> GetAll() => _repository.GetAll();


    // Valida y crea el deposito: monto > 0, comprobante presente y con extension/tamano permitidos.
    // Devuelve false con un mensaje en 'error' cuando la validacion falla.
    public bool TryCreateDeposit(decimal amount, Stream? receiptStream, string? fileName, out string? error)
    {
        error = null;

        if (amount <= 0)
        {
            error = "El monto debe ser mayor a cero.";
            return false;
        }

        if (receiptStream is null || receiptStream.Length == 0)
        {
            error = "Debes adjuntar el comprobante de la transferencia.";
            return false;
        }

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension) || receiptStream.Length > MaxFileSizeBytes)
        {
            error = "El comprobante debe ser JPEG, PNG o PDF y no superar los 5 MB.";
            return false;
        }

        var deposit = _repository.Add(new DepositRequest
        {
            Amount = amount,
            RequestedAtUtc = DateTime.UtcNow,
            Status = "Pendiente de validacion"
        });

        var savedFileName = $"deposit-{deposit.Id}{extension}";
        _fileStorage.Save(receiptStream, savedFileName);
        deposit.ReceiptFileName = savedFileName;

        return true;
    }
}