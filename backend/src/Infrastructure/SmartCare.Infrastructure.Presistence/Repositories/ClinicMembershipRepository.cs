using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class ClinicMembershipRepository : IClinicMembershipRepository
{
    private readonly SmartCareDbContext _context;
    public ClinicMembershipRepository(SmartCareDbContext context) => _context = context;

    public Task<ClinicMembership?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.ClinicMemberships.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<bool> ExistsAsync(Guid clinicId, Guid doctorId, CancellationToken ct = default) =>
        _context.ClinicMemberships.AnyAsync(m => m.ClinicId == clinicId && m.DoctorId == doctorId, ct);

    public async Task AddAsync(ClinicMembership membership, CancellationToken ct = default) =>
        await _context.ClinicMemberships.AddAsync(membership, ct);
    public async Task<IReadOnlyList<ClinicMembership>> GetByClinicAndDepartmentAsync(
    Guid clinicId, Guid? departmentId, bool activeOnly, CancellationToken ct = default)
    {
        var query = _context.ClinicMemberships.Where(m => m.ClinicId == clinicId);

        if (activeOnly)
            query = query.Where(m => m.IsActive);

        if (departmentId is not null)
            query = query.Where(m => m.DepartmentId == departmentId);

        return await query.ToListAsync(ct);
    }
    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public Task<ClinicMembership?> GetByClinicAndDoctorAsync(Guid clinicId, Guid doctorId, CancellationToken ct = default) =>
    _context.ClinicMemberships.FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.DoctorId == doctorId, ct);
}
