using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.ExpireAppointment;

public class ExpireAppointmentCommandHandler : IRequestHandler<ExpireAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;

    public ExpireAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
    }

    public async Task<Result> Handle(ExpireAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        var previousStatus = appointment.Status;
        try { appointment.Expire(); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _appointmentRepository.SaveChangesAsync(ct);

        if (appointment.ScheduleSlotId is Guid slotId)
        {
            var slot = await _slotRepository.GetByIdAsync(slotId, ct);
            slot?.Cancel(); // same cascade as CancelAppointmentCommandHandler
            await _slotRepository.SaveChangesAsync(ct);
        }

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, previousStatus, appointment.Status,
                null, "Payment window expired"), ct); // null = system action, not a specific user

        await _historyRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
