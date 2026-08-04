using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.RequestRefund;

public class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Result<Guid>>
{
    private readonly IRefundRequestRepository _refundRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public RequestRefundCommandHandler(IRefundRequestRepository refundRepository, IAppointmentRepository appointmentRepository)
    {
        _refundRepository = refundRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<Guid>> Handle(RequestRefundCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, ct);
        if (appointment is null) return Result<Guid>.Failure("Appointment not found.");
        if (appointment.PaymentStatus != PaymentStatus.Verified)
            return Result<Guid>.Failure("No verified payment exists for this appointment to refund.");

        var refund = RefundRequest.Create(request.AppointmentId, request.RequestedAmount, request.Reason, request.RequestedByUserId);

        await _refundRepository.AddAsync(refund, ct);
        await _refundRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(refund.Id);
    }
}
