using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.SetClinicPolicy;

public class SetClinicPolicyCommandHandler : IRequestHandler<SetClinicPolicyCommand, Result<Guid>>
{
    private readonly IClinicPolicyRepository _policyRepository;
    public SetClinicPolicyCommandHandler(IClinicPolicyRepository policyRepository) =>
        _policyRepository = policyRepository;

    public async Task<Result<Guid>> Handle(SetClinicPolicyCommand request, CancellationToken ct)
    {
        var currentPolicy = await _policyRepository.GetCurrentByClinicIdAsync(request.ClinicId, ct);
        currentPolicy?.Close(DateTime.UtcNow); // close the old version, if one exists

        var newPolicy = ClinicPolicy.Create(
            request.ClinicId, request.AdvancePaymentRequired, request.DepositPercentage,
            request.CancellationWindowHours, request.RefundPercentage, request.NoShowPenaltyAmount,
            request.BookingWindowDays, request.MaxDailyBookingsPerPatient, request.WalkInBookingAllowed,
            request.ConfirmationRequired, request.LateArrivalGraceMinutes, request.MinAttendancePercentage,
            request.AllowedReschedule, request.MaxReschedule);

        await _policyRepository.AddAsync(newPolicy, ct);
        await _policyRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(newPolicy.Id);
    }
}