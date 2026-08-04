using SmartCare.SharedKernel;

namespace SmartCare.Domain.Appointments;

public class Appointment : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid PatientProfileId { get; private set; }
    public Guid DoctorId { get; private set; }          // Stores ClinicMembership.Id — see note above
    public Guid? DepartmentId { get; private set; }
    public Guid? ScheduleSlotId { get; private set; }
    public DateTime BookingDateUtc { get; private set; } = DateTime.UtcNow;
    public DateOnly AppointmentDate { get; private set; }
    public TimeOnly AppointmentTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public decimal FeeAtBooking { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }

    // Payment fields (Option A — folded onto Appointments, no separate Payments table)
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.NotPaid;
    public string? PaymentProofUrl { get; private set; }
    public string? PaymentMethod { get; private set; }
    public Guid? PaymentVerifiedByUserId { get; private set; }
    public DateTime? PaymentVerifiedAtUtc { get; private set; }

    private Appointment() { }

    public static Appointment Book(Guid clinicId, Guid patientProfileId, Guid doctorId, Guid? departmentId,
        Guid? scheduleSlotId, DateOnly appointmentDate, TimeOnly appointmentTime, decimal feeAtBooking, string? notes)
    {
        if (feeAtBooking < 0) throw new ArgumentException("Fee cannot be negative.");

        return new Appointment
        {
            ClinicId = clinicId,
            PatientProfileId = patientProfileId,
            DoctorId = doctorId,
            DepartmentId = departmentId,
            ScheduleSlotId = scheduleSlotId,
            AppointmentDate = appointmentDate,
            AppointmentTime = appointmentTime,
            FeeAtBooking = feeAtBooking,
            Notes = notes,
            Status = AppointmentStatus.Pending
        };
    }

    public void SubmitPaymentProof(string proofUrl, string paymentMethod)
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException($"Cannot submit payment for an appointment in status '{Status}'.");
        if (string.IsNullOrWhiteSpace(proofUrl))
            throw new ArgumentException("Payment proof is required.");

        PaymentProofUrl = proofUrl;
        PaymentMethod = paymentMethod;
        PaymentStatus = PaymentStatus.AwaitingVerification;
    }

    public void VerifyPayment(Guid verifiedByUserId)
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException($"Cannot verify payment for an appointment in status '{Status}'.");
        if (PaymentStatus != PaymentStatus.AwaitingVerification)
            throw new InvalidOperationException("No payment proof is awaiting verification.");

        PaymentStatus = PaymentStatus.Verified;
        PaymentVerifiedByUserId = verifiedByUserId;
        PaymentVerifiedAtUtc = DateTime.UtcNow;
        Status = AppointmentStatus.Confirmed;
    }

    public void RejectPayment(Guid rejectedByUserId, string? reason)
    {
        if (PaymentStatus != PaymentStatus.AwaitingVerification)
            throw new InvalidOperationException("No payment proof is awaiting verification.");

        PaymentStatus = PaymentStatus.Rejected;
        PaymentVerifiedByUserId = rejectedByUserId;
        PaymentVerifiedAtUtc = DateTime.UtcNow;
        // Status stays Pending — patient can re-upload a new screenshot via SubmitPaymentProof again.
    }

    public void CheckIn()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException($"Cannot check in an appointment in status '{Status}'.");
        Status = AppointmentStatus.CheckedIn;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.CheckedIn)
            throw new InvalidOperationException($"Cannot complete an appointment in status '{Status}'.");
        Status = AppointmentStatus.Completed;
    }

    public void Cancel(string? reason)
    {
        if (Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new InvalidOperationException($"Cannot cancel an appointment in status '{Status}'.");

        Status = AppointmentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void Reject(string? reason)
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException($"Cannot reject an appointment in status '{Status}'.");

        Status = AppointmentStatus.Rejected;
        CancellationReason = reason;
    }

    public void MarkNoShow()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException($"Cannot mark no-show for an appointment in status '{Status}'.");
        Status = AppointmentStatus.NoShow;
    }

    public void Expire()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException($"Cannot expire an appointment in status '{Status}'.");
        Status = AppointmentStatus.Expired;
    }
}