using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly SmartCareDbContext _context;
    public AppointmentRepository(SmartCareDbContext context) => _context = context;

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Appointment>> GetByPatientAsync(Guid patientProfileId, CancellationToken ct = default) =>
        await _context.Appointments
            .Where(a => a.PatientProfileId == patientProfileId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetPendingAwaitingVerificationAsync(Guid clinicId, CancellationToken ct = default) =>
        await _context.Appointments
            .Where(a => a.ClinicId == clinicId
                && a.Status == AppointmentStatus.Pending
                && a.PaymentStatus == PaymentStatus.AwaitingVerification)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
            .ToListAsync(ct);

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default) =>
        await _context.Appointments.AddAsync(appointment, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public async Task<IReadOnlyList<Appointment>> GetByClinicAndDateAsync(
    Guid clinicId, DateOnly date, CancellationToken ct = default) =>
    await _context.Appointments
        .Where(a => a.ClinicId == clinicId && a.AppointmentDate == date)
        .OrderBy(a => a.AppointmentTime)
        .ToListAsync(ct);

}
