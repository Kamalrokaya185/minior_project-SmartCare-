namespace SmartCare.Domain.ClinicalDirectory;

public interface IDoctorScheduleRepository
{
    Task<IReadOnlyList<DoctorSchedule>> GetApplicableSchedulesAsync(
        Guid clinicMembershipId, DateOnly date, CancellationToken ct = default);
    Task AddAsync(DoctorSchedule schedule, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}