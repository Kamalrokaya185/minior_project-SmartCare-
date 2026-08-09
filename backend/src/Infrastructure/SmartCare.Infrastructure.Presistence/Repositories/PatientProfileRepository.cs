using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Patients;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class PatientProfileRepository : IPatientProfileRepository
{
    private readonly SmartCareDbContext _context;
    public PatientProfileRepository(SmartCareDbContext context) => _context = context;

    public Task<PatientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<PatientProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.PatientProfiles.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(PatientProfile profile, CancellationToken ct = default) =>
        await _context.PatientProfiles.AddAsync(profile, ct);
    public async Task<IReadOnlyList<PatientProfile>> GetAllAsync(CancellationToken ct = default) =>
    await _context.PatientProfiles.OrderByDescending(p => p.CreatedAtUtc).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public async Task<(int Total, int Active, int Inactive)> GetStatusCountsAsync(CancellationToken ct = default)
    {
        var query = from p in _context.PatientProfiles
                    join u in _context.Users on p.UserId equals u.Id
                    select u.IsActive;

        var total = await query.CountAsync(ct);
        var active = await query.CountAsync(isActive => isActive, ct);
        var inactive = total - active;
        return (total, active, inactive);
    }
}
