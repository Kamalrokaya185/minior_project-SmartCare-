using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.SubmitPaymentProof;

public class SubmitPaymentProofCommandHandler : IRequestHandler<SubmitPaymentProofCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    public SubmitPaymentProofCommandHandler(IAppointmentRepository appointmentRepository) =>
        _appointmentRepository = appointmentRepository;

    public async Task<Result> Handle(SubmitPaymentProofCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result.Failure("Appointment not found.");

        try
        {
            appointment.SubmitPaymentProof(request.PaymentProofUrl, request.PaymentMethod);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result.Failure(ex.Message);
        }

        await _appointmentRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}