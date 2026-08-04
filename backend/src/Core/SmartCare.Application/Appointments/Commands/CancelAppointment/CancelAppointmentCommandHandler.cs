using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        var previousStatus = appointment.Status;
        try { appointment.Cancel(request.Reason); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _appointmentRepository.SaveChangesAsync(ct);

        if (appointment.ScheduleSlotId is Guid slotId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId, ct);
            slot?.Cancel();
            await _slotRepository.SaveChangesAsync(ct);
        }

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, previousStatus, appointment.Status,
                request.ChangedByUserId, request.Reason ?? "Cancelled"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}