using System;
using System.Collections.Generic;
using System.Text;

// PatientProfileRepository.cs
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

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
