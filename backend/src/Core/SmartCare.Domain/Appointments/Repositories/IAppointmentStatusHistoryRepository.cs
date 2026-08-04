namespace SmartCare.Domain.Appointments;

public interface IAppointmentStatusHistoryRepository
{
    Task AddAsync(AppointmentStatusHistoryEntry entry, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
