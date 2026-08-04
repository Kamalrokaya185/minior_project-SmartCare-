using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.SetClinicPolicy;

public record SetClinicPolicyCommand(
    Guid ClinicId, bool AdvancePaymentRequired, decimal DepositPercentage,
    int CancellationWindowHours, decimal RefundPercentage, decimal NoShowPenaltyAmount,
    int BookingWindowDays, int MaxDailyBookingsPerPatient, bool WalkInBookingAllowed,
    bool ConfirmationRequired, int LateArrivalGraceMinutes, decimal MinAttendancePercentage,
    bool AllowedReschedule, int MaxReschedule) : IRequest<Result<Guid>>;