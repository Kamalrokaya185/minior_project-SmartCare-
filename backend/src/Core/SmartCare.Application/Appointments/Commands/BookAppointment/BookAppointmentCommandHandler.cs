using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.BookAppointment;

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;
    private readonly IClinicMembershipRepository _membershipRepository;

    public BookAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository,
        IClinicMembershipRepository membershipRepository)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken ct)
    {
        var slot = await _slotRepository.GetByIdAsync(request.ScheduleSlotId, ct);
        if (slot is null) return Result<Guid>.Failure("Reserved slot not found.");

        if (slot.IsReservationExpired())
        {
            slot.Cancel(); // actively swap it now that we've noticed — matches your "system checks then swaps" idea
            await _slotRepository.SaveChangesAsync(ct);
            return Result<Guid>.Failure("Your slot reservation has expired. Please select a slot again.");
        }


        var membership = await _membershipRepository.GetByIdAsync(request.ClinicMembershipId, ct);
        if (membership is null) return Result<Guid>.Failure("Doctor's clinic membership not found.");

        var appointment = Appointment.Book(
            request.ClinicId, request.PatientProfileId, request.ClinicMembershipId, request.DepartmentId,
            request.ScheduleSlotId, request.AppointmentDate, request.AppointmentTime,
            membership.ConsultationFee ?? 0m, request.Notes);

        await _appointmentRepository.AddAsync(appointment, ct);

        try
        {
            slot.ConfirmBooking(); // Reserved -> Booked; throws if slot isn't actually Reserved anymore
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        await _appointmentRepository.SaveChangesAsync(ct);
        await _slotRepository.SaveChangesAsync(ct);

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, null, AppointmentStatus.Pending,
                request.PatientProfileId, "Appointment booked"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(appointment.Id);
    }
}
