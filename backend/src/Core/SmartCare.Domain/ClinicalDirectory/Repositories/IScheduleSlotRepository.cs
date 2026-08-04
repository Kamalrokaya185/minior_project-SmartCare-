namespace SmartCare.Domain.ClinicalDirectory;

public interface IScheduleSlotRepository
{
    /// <summary>Times that currently count as taken: Booked/Completed/Blocked, or Reserved-and-not-yet-expired.</summary>
    Task<IReadOnlyList<TimeOnly>> GetTakenStartTimesAsync(
        Guid clinicMembershipId, DateOnly date, CancellationToken ct = default);

    Task<ScheduleSlot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ScheduleSlot slot, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<ScheduleSlot?> GetByClinicMembershipDateTimeAsync(
    Guid clinicMembershipId, DateOnly date, TimeOnly startTime, CancellationToken ct = default);

}