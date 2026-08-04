using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class AppointmentStatusHistoryRepository : IAppointmentStatusHistoryRepository
{
    private readonly SmartCareDbContext _context;
    public AppointmentStatusHistoryRepository(SmartCareDbContext context) => _context = context;

    public async Task AddAsync(AppointmentStatusHistoryEntry entry, CancellationToken ct = default) =>
        await _context.AppointmentStatusHistory.AddAsync(entry, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}