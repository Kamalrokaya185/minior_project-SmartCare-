namespace SmartCare.Domain.Appointments;

public interface IRefundRequestRepository
{
    Task<RefundRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(RefundRequest request, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}