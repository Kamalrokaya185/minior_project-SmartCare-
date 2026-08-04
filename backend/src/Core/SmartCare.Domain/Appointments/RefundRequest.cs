using SmartCare.SharedKernel;

namespace SmartCare.Domain.Appointments;

public class RefundRequest : AggregateRoot
{
    public Guid AppointmentId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public decimal? ApprovedAmount { get; private set; }
    public string? Reason { get; private set; }
    public RefundStatus Status { get; private set; } = RefundStatus.Pending;
    public Guid RequestedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime RequestedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; private set; }

    private RefundRequest() { }

    public static RefundRequest Create(Guid appointmentId, decimal requestedAmount, string? reason, Guid requestedByUserId)
    {
        if (requestedAmount < 0) throw new ArgumentException("Requested amount cannot be negative.");

        return new RefundRequest
        {
            AppointmentId = appointmentId,
            RequestedAmount = requestedAmount,
            Reason = reason,
            RequestedByUserId = requestedByUserId
        };
    }

    public void Approve(decimal approvedAmount, Guid approvedByUserId)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot approve a refund request in status '{Status}'.");
        if (approvedAmount < 0 || approvedAmount > RequestedAmount)
            throw new ArgumentException("Approved amount must be between 0 and the requested amount.");

        Status = RefundStatus.Approved;
        ApprovedAmount = approvedAmount;
        ApprovedByUserId = approvedByUserId;
    }

    public void Reject(Guid rejectedByUserId)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot reject a refund request in status '{Status}'.");

        Status = RefundStatus.Rejected;
        ApprovedByUserId = rejectedByUserId;
    }

    public void MarkProcessed()
    {
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException("Only an approved refund can be marked processed.");

        Status = RefundStatus.Processed;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
