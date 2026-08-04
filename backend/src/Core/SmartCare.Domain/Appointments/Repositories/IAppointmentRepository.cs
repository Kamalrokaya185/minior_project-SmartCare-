namespace SmartCare.Domain.Appointments;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByPatientAsync(Guid patientProfileId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetPendingAwaitingVerificationAsync(Guid clinicId, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
