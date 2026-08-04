namespace SmartCare.Domain.Tenancy;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Department?> GetByClinicAndNameAsync(Guid clinicId, string name, CancellationToken ct = default);
    Task<IReadOnlyList<Department>> GetByClinicAsync(Guid clinicId, CancellationToken ct = default);
    Task AddAsync(Department department, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
