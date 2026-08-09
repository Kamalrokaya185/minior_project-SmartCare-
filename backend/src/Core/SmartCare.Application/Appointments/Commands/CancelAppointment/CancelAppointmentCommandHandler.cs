using MediatR;
using SmartCare.Application.Appointments.Services;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;
    private readonly IClinicPolicyRepository _policyRepository;
    private readonly IRefundRequestRepository _refundRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository,
        IClinicPolicyRepository policyRepository,
        IRefundRequestRepository refundRepository,
        IPatientProfileRepository patientProfileRepository,
        ICurrentUserService currentUserService)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
        _policyRepository = policyRepository;
        _refundRepository = refundRepository;
        _patientProfileRepository = patientProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        if (_currentUserService.UserId is not Guid userId)
            return Result.Failure("You must be logged in.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        // Ownership check — a patient may only cancel their own appointment (Clinic/SuperAdmin bypass this via a separate admin action, not built here)
        var patientProfile = await _patientProfileRepository.GetByUserIdAsync(userId, ct);
        if (patientProfile is null || appointment.PatientProfileId != patientProfile.Id)
            return Result.Failure("You are not authorized to cancel this appointment.");

        var previousStatus = appointment.Status;
        var wasPaymentVerified = appointment.PaymentStatus == PaymentStatus.Verified;

        try { appointment.Cancel(request.Reason); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _appointmentRepository.SaveChangesAsync(ct);

        if (appointment.ScheduleSlotId is Guid slotId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId, ct);
            slot?.Cancel();
            await _slotRepository.SaveChangesAsync(ct);
        }

        // Refund eligibility — only relevant if money actually changed hands
        if (wasPaymentVerified)
        {
            var policy = await _policyRepository.GetCurrentByClinicIdAsync(appointment.ClinicId, ct);
            var eligibleAmount = RefundCalculationService.CalculateEligibleRefund(appointment, policy, DateTime.UtcNow);

            if (eligibleAmount is > 0)
            {
                var refund = RefundRequest.Create(appointment.Id, eligibleAmount.Value,
                    "Automatic — eligible per clinic cancellation policy", userId);
                await _refundRepository.AddAsync(refund, ct);
                await _refundRepository.SaveChangesAsync(ct);
            }
            // eligibleAmount null or 0 -> cancelled outside the policy window; no RefundRequest created,
            // matching "Do not automatically assume every cancellation receives a refund."
        }

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, previousStatus, appointment.Status,
                userId, request.Reason ?? "Cancelled by patient"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
