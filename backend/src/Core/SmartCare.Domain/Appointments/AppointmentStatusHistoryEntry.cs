using SmartCare.SharedKernel;

namespace SmartCare.Domain.Appointments;

public class AppointmentStatusHistoryEntry : Entity
{
    public Guid AppointmentId { get; private set; }
    public AppointmentStatus? FromStatus { get; private set; }
    public AppointmentStatus ToStatus { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAtUtc { get; private set; } = DateTime.UtcNow;

    private AppointmentStatusHistoryEntry() { }

    public static AppointmentStatusHistoryEntry Create(Guid appointmentId, AppointmentStatus? fromStatus,
        AppointmentStatus toStatus, Guid? changedByUserId, string? reason)
    {
        return new AppointmentStatusHistoryEntry
        {
            AppointmentId = appointmentId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            Reason = reason
        };
    }
}
