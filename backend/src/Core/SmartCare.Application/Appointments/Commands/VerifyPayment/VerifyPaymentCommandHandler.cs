using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.VerifyPayment;

public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;

    public VerifyPaymentCommandHandler(
        IAppointmentRepository appointmentRepository, IAppointmentStatusHistoryRepository historyRepository)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
    }

    public async Task<Result> Handle(VerifyPaymentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        var previousStatus = appointment.Status;

        try
        {
            appointment.VerifyPayment(request.VerifiedByUserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _appointmentRepository.SaveChangesAsync(ct);

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, previousStatus, appointment.Status,
                request.VerifiedByUserId, "Payment verified"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
