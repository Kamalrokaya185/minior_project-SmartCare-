using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.ClinicalDirectory;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class DoctorProfileRepository : IDoctorProfileRepository
{
    private readonly SmartCareDbContext _context;
    public DoctorProfileRepository(SmartCareDbContext context) => _context = context;

    public Task<DoctorProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.DoctorProfiles.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DoctorProfile?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct = default) =>
        _context.DoctorProfiles.FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber, ct);

    public async Task AddAsync(DoctorProfile profile, CancellationToken ct = default) =>
        await _context.DoctorProfiles.AddAsync(profile, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}

