using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.DecideRefund;

public class DecideRefundCommandHandler : IRequestHandler<DecideRefundCommand, Result>
{
    private readonly IRefundRequestRepository _refundRepository;
    public DecideRefundCommandHandler(IRefundRequestRepository refundRepository) => _refundRepository = refundRepository;

    public async Task<Result> Handle(DecideRefundCommand request, CancellationToken ct)
    {
        var refund = await _refundRepository.GetByIdAsync(request.RefundRequestId, ct);
        if (refund is null) return Result.Failure("Refund request not found.");

        try
        {
            if (request.Approve)
                refund.Approve(request.ApprovedAmount ?? refund.RequestedAmount, request.DecidedByUserId);
            else
                refund.Reject(request.DecidedByUserId);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result.Failure(ex.Message);
        }

        await _refundRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
