namespace SmartCare.Domain.ClinicalDirectory;

public interface IDoctorProfileRepository
{
    Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DoctorProfile?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct = default);
    Task AddAsync(DoctorProfile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
