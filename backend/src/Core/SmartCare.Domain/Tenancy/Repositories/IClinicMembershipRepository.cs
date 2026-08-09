namespace SmartCare.Domain.Tenancy;

public interface IClinicMembershipRepository
{
    Task<ClinicMembership?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid clinicId, Guid doctorId, CancellationToken ct = default);
    Task AddAsync(ClinicMembership membership, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ClinicMembership>> GetByClinicAndDepartmentAsync(
    Guid clinicId, Guid? departmentId, bool activeOnly, CancellationToken ct = default);
    Task<ClinicMembership?> GetByClinicAndDoctorAsync(Guid clinicId, Guid doctorId, CancellationToken ct = default);
}
