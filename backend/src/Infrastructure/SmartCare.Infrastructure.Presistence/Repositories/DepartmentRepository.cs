using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly SmartCareDbContext _context;
    public DepartmentRepository(SmartCareDbContext context) => _context = context;

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Departments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<Department?> GetByClinicAndNameAsync(Guid clinicId, string name, CancellationToken ct = default) =>
        _context.Departments.FirstOrDefaultAsync(d => d.ClinicId == clinicId && d.Name == name, ct);

    public async Task<IReadOnlyList<Department>> GetByClinicAsync(Guid clinicId, CancellationToken ct = default) =>
        await _context.Departments.Where(d => d.ClinicId == clinicId && d.IsActive).ToListAsync(ct);

    public async Task AddAsync(Department department, CancellationToken ct = default) =>
        await _context.Departments.AddAsync(department, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
