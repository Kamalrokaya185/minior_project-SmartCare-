using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.RejectPayment;

public class RejectPaymentCommandHandler : IRequestHandler<RejectPaymentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    public RejectPaymentCommandHandler(IAppointmentRepository appointmentRepository) =>
        _appointmentRepository = appointmentRepository;

    public async Task<Result> Handle(RejectPaymentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        try
        {
            appointment.RejectPayment(request.RejectedByUserId, request.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _appointmentRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
