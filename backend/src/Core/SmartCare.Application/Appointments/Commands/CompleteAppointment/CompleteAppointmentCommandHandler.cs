using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;

    public CompleteAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
    }

    public async Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        var previousStatus = appointment.Status;
        try { appointment.Complete(); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _appointmentRepository.SaveChangesAsync(ct);

        if (appointment.ScheduleSlotId is Guid slotId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId, ct);
            slot?.Complete();
            await _slotRepository.SaveChangesAsync(ct);
        }

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, previousStatus, appointment.Status,
                request.ChangedByUserId, "Consultation completed"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
