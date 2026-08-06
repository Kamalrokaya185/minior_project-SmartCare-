using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly SmartCareDbContext _context;
    public ClinicRepository(SmartCareDbContext context) => _context = context;

    public Task<Clinic?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Clinics.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Clinic?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _context.Clinics.FirstOrDefaultAsync(c => c.Slug == slug.ToLower(), ct);

    public async Task AddAsync(Clinic clinic, CancellationToken ct = default) =>
        await _context.Clinics.AddAsync(clinic, ct);
    public async Task<IReadOnlyList<Clinic>> GetAllAsync(CancellationToken ct = default) =>
    await _context.Clinics.OrderByDescending(c => c.CreatedAtUtc).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
