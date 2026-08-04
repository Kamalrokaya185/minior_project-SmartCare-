using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.DecideRefund;

public record DecideRefundCommand(Guid RefundRequestId, bool Approve, decimal? ApprovedAmount, Guid DecidedByUserId)
    : IRequest<Result>;
