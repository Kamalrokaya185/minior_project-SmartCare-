using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.ClinicalDirectory;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class ScheduleSlotRepository : IScheduleSlotRepository
{
    private readonly SmartCareDbContext _context;
    public ScheduleSlotRepository(SmartCareDbContext context) => _context = context;

    public async Task<IReadOnlyList<TimeOnly>> GetTakenStartTimesAsync(
        Guid clinicMembershipId, DateOnly date, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return await _context.ScheduleSlots
            .Where(s => s.ClinicMembershipId == clinicMembershipId && s.SlotDate == date)
            .Where(s =>
                s.Status == SlotStatus.Booked ||
                s.Status == SlotStatus.Completed ||
                s.Status == SlotStatus.Blocked ||
                (s.Status == SlotStatus.Reserved && s.ReservedUntilUtc != null && s.ReservedUntilUtc > now))
            .Select(s => s.StartTime)
            .ToListAsync(ct);
    }

    public Task<ScheduleSlot?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.ScheduleSlots.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(ScheduleSlot slot, CancellationToken ct = default) =>
        await _context.ScheduleSlots.AddAsync(slot, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public Task<ScheduleSlot?> GetByClinicMembershipDateTimeAsync(
    Guid clinicMembershipId, DateOnly date, TimeOnly startTime, CancellationToken ct = default) =>
    _context.ScheduleSlots.FirstOrDefaultAsync(s =>
        s.ClinicMembershipId == clinicMembershipId && s.SlotDate == date && s.StartTime == startTime, ct);
}
