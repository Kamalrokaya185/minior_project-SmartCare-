using SmartCare.SharedKernel;

namespace SmartCare.Domain.ClinicalDirectory;

public class ScheduleSlot : AggregateRoot
{
    public Guid? DoctorScheduleId { get; private set; }   // Null if manually blocked/created without a schedule template
    public Guid ClinicMembershipId { get; private set; }  // Denormalized — see chunk-2 discussion, this is what the unique constraint uses
    public DateOnly SlotDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public SlotStatus Status { get; private set; }
    public DateTime? ReservedUntilUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private ScheduleSlot() { }

    public static ScheduleSlot Reserve(Guid clinicMembershipId, Guid? doctorScheduleId,
        DateOnly slotDate, TimeOnly startTime, TimeOnly endTime, int holdMinutes = 10)
    {
        return new ScheduleSlot
        {
            ClinicMembershipId = clinicMembershipId,
            DoctorScheduleId = doctorScheduleId,
            SlotDate = slotDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = SlotStatus.Reserved,
            ReservedUntilUtc = DateTime.UtcNow.AddMinutes(holdMinutes)
        };
    }

    public static ScheduleSlot Block(Guid clinicMembershipId, DateOnly slotDate, TimeOnly startTime, TimeOnly endTime)
    {
        return new ScheduleSlot
        {
            ClinicMembershipId = clinicMembershipId,
            SlotDate = slotDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = SlotStatus.Blocked
        };
    }

    public void ConfirmBooking()
    {
        if (Status != SlotStatus.Reserved)
            throw new InvalidOperationException($"Cannot confirm booking for a slot in status '{Status}'.");
        Status = SlotStatus.Booked;
        ReservedUntilUtc = null;
    }

    public void Complete()
    {
        if (Status != SlotStatus.Booked)
            throw new InvalidOperationException($"Cannot complete a slot in status '{Status}'.");
        Status = SlotStatus.Completed;
    }
    public void Cancel()
    {
        if (Status is SlotStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed slot.");

        Status = SlotStatus.Cancelled;
        ReservedUntilUtc = null;
    }

    /// <summary>Re-reserves a previously cancelled/expired slot instead of inserting a duplicate row.</summary>
    public void ReReserve(int holdMinutes = 10)
    {
        if (Status != SlotStatus.Cancelled && !IsReservationExpired())
            throw new InvalidOperationException($"Cannot re-reserve a slot in status '{Status}'.");

        Status = SlotStatus.Reserved;
        ReservedUntilUtc = DateTime.UtcNow.AddMinutes(holdMinutes);
    }

    public bool IsReservationExpired() => Status == SlotStatus.Reserved
        && ReservedUntilUtc.HasValue && ReservedUntilUtc.Value <= DateTime.UtcNow;
}