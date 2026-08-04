namespace SmartCare.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken ct = default);
}
