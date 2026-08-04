namespace SmartCare.Domain.Tenancy;

public interface IClinicPolicyRepository
{
    Task<ClinicPolicy?> GetCurrentByClinicIdAsync(Guid clinicId, CancellationToken ct = default);
    Task AddAsync(ClinicPolicy policy, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}