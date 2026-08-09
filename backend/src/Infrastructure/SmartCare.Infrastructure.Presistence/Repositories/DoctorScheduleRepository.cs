using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.ClinicalDirectory;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class DoctorScheduleRepository : IDoctorScheduleRepository
{
    private readonly SmartCareDbContext _context;
    public DoctorScheduleRepository(SmartCareDbContext context) => _context = context;

    public async Task<IReadOnlyList<DoctorSchedule>> GetApplicableSchedulesAsync(
        Guid clinicMembershipId, DateOnly date, CancellationToken ct = default)
    {
        var dayOfWeek = SlotAvailabilityService.ToScheduleDayOfWeek(date);

        return await _context.DoctorSchedules
            .Where(s => s.ClinicMembershipId == clinicMembershipId
                && s.IsActive
                && s.EffectiveFrom <= date
                && (s.EffectiveTo == null || s.EffectiveTo >= date)
                && (
                    (s.IsRecurring && s.DayOfWeek == dayOfWeek) ||
                    (!s.IsRecurring && s.SpecificDate == date)
                ))
            .ToListAsync(ct);
    }

    public async Task AddAsync(DoctorSchedule schedule, CancellationToken ct = default) =>
        await _context.DoctorSchedules.AddAsync(schedule, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<DoctorSchedule>> GetAllByMembershipAsync(Guid clinicMembershipId, CancellationToken ct = default) =>
    await _context.DoctorSchedules
        .Where(s => s.ClinicMembershipId == clinicMembershipId)
        .OrderBy(s => s.DayOfWeek).ThenBy(s => s.SpecificDate)
        .ToListAsync(ct);
    public Task<DoctorSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
    _context.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id, ct);

}