// IClinicRepository.cs
namespace SmartCare.Domain.Tenancy;

public interface IClinicRepository
{
    Task<Clinic?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Clinic?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Clinic clinic, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
