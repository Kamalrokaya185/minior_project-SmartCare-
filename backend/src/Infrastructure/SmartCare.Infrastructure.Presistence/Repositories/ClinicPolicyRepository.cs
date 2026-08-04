using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class ClinicPolicyRepository : IClinicPolicyRepository
{
    private readonly SmartCareDbContext _context;
    public ClinicPolicyRepository(SmartCareDbContext context) => _context = context;

    public Task<ClinicPolicy?> GetCurrentByClinicIdAsync(Guid clinicId, CancellationToken ct = default) =>
        _context.ClinicPolicies.FirstOrDefaultAsync(p => p.ClinicId == clinicId && p.IsCurrent, ct);

    public async Task AddAsync(ClinicPolicy policy, CancellationToken ct = default) =>
        await _context.ClinicPolicies.AddAsync(policy, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}