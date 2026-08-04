using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.RequestRefund;

public record RequestRefundCommand(Guid AppointmentId, decimal RequestedAmount, string? Reason, Guid RequestedByUserId)
    : IRequest<Result<Guid>>;
