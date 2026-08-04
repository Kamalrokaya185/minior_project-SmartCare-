using SmartCare.SharedKernel;

namespace SmartCare.Domain.Tenancy;

public class ClinicPolicy : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public bool AdvancePaymentRequired { get; private set; } = true;
    public decimal DepositPercentage { get; private set; } = 100m;
    public int CancellationWindowHours { get; private set; } = 24;
    public decimal RefundPercentage { get; private set; } = 100m;
    public decimal NoShowPenaltyAmount { get; private set; } = 0m;
    public int BookingWindowDays { get; private set; } = 30;
    public int MaxDailyBookingsPerPatient { get; private set; } = 3;
    public bool WalkInBookingAllowed { get; private set; } = true;
    public bool ConfirmationRequired { get; private set; } = true;
    public int LateArrivalGraceMinutes { get; private set; } = 15;
    public decimal MinAttendancePercentage { get; private set; } = 50m;
    public bool AllowedReschedule { get; private set; } = true;
    public int MaxReschedule { get; private set; } = 1;
    public DateTime EffectiveFromUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? EffectiveToUtc { get; private set; }
    public bool IsCurrent { get; private set; } = true;

    private ClinicPolicy() { }

    public static ClinicPolicy Create(Guid clinicId, bool advancePaymentRequired, decimal depositPercentage,
        int cancellationWindowHours, decimal refundPercentage, decimal noShowPenaltyAmount,
        int bookingWindowDays, int maxDailyBookingsPerPatient, bool walkInBookingAllowed,
        bool confirmationRequired, int lateArrivalGraceMinutes, decimal minAttendancePercentage,
        bool allowedReschedule, int maxReschedule)
    {
        if (depositPercentage is < 0 or > 100) throw new ArgumentException("Deposit percentage must be 0-100.");
        if (refundPercentage is < 0 or > 100) throw new ArgumentException("Refund percentage must be 0-100.");

        return new ClinicPolicy
        {
            ClinicId = clinicId,
            AdvancePaymentRequired = advancePaymentRequired,
            DepositPercentage = depositPercentage,
            CancellationWindowHours = cancellationWindowHours,
            RefundPercentage = refundPercentage,
            NoShowPenaltyAmount = noShowPenaltyAmount,
            BookingWindowDays = bookingWindowDays,
            MaxDailyBookingsPerPatient = maxDailyBookingsPerPatient,
            WalkInBookingAllowed = walkInBookingAllowed,
            ConfirmationRequired = confirmationRequired,
            LateArrivalGraceMinutes = lateArrivalGraceMinutes,
            MinAttendancePercentage = minAttendancePercentage,
            AllowedReschedule = allowedReschedule,
            MaxReschedule = maxReschedule
        };
    }

    public void Close(DateTime effectiveToUtc)
    {
        IsCurrent = false;
        EffectiveToUtc = effectiveToUtc;
    }
}