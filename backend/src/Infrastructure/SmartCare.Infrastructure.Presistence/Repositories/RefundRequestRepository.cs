using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class RefundRequestRepository : IRefundRequestRepository
{
    private readonly SmartCareDbContext _context;
    public RefundRequestRepository(SmartCareDbContext context) => _context = context;

    public Task<RefundRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.RefundRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(RefundRequest request, CancellationToken ct = default) =>
        await _context.RefundRequests.AddAsync(request, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}