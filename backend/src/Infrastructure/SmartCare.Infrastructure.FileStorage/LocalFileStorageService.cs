using SmartCare.Application.Common.Interfaces;

namespace SmartCare.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private static readonly HashSet<string> AllowedContentTypes = new() { "image/jpeg", "image/png", "image/webp" };

    public LocalFileStorageService(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken ct = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException("Only JPEG, PNG, or WEBP images are allowed for payment proof.");

        var extension = Path.GetExtension(originalFileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}"; // random name — never trust the original filename

        var paymentProofPath = Path.Combine(_rootPath, "payment-proofs");
        Directory.CreateDirectory(paymentProofPath);

        var fullPath = Path.Combine(paymentProofPath, safeFileName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, ct);

        return $"/uploads/payment-proofs/{safeFileName}";
    }
}
