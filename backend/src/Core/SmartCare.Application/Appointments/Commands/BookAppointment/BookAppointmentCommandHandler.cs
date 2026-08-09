using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.BookAppointment;

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentStatusHistoryRepository _historyRepository;
    private readonly IScheduleSlotRepository _slotRepository;
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public BookAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentStatusHistoryRepository historyRepository,
        IScheduleSlotRepository slotRepository,
        IClinicMembershipRepository membershipRepository,
        IPatientProfileRepository patientProfileRepository,
        ICurrentUserService currentUserService)
    {
        _appointmentRepository = appointmentRepository;
        _historyRepository = historyRepository;
        _slotRepository = slotRepository;
        _membershipRepository = membershipRepository;
        _patientProfileRepository = patientProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken ct)
    {
        if (_currentUserService.UserId is not Guid userId)
            return Result<Guid>.Failure("You must be logged in to book an appointment.");

        var patientProfile = await _patientProfileRepository.GetByUserIdAsync(userId, ct);
        if (patientProfile is null)
            return Result<Guid>.Failure("No patient profile found for this account.");

        var slot = await _slotRepository.GetByIdAsync(request.ScheduleSlotId, ct);
        if (slot is null) return Result<Guid>.Failure("Reserved slot not found.");
        if (slot.ClinicMembershipId != request.ClinicMembershipId)
            return Result<Guid>.Failure("This slot does not belong to the selected doctor."); // ownership validation
        if (slot.IsReservationExpired())
        {
            slot.Cancel();
            await _slotRepository.SaveChangesAsync(ct);
            return Result<Guid>.Failure("Your slot reservation has expired. Please select a slot again.");
        }

        var membership = await _membershipRepository.GetByIdAsync(request.ClinicMembershipId, ct);
        if (membership is null || !membership.IsActive)
            return Result<Guid>.Failure("This doctor is not currently available at this clinic.");
        if (membership.ClinicId != request.ClinicId)
            return Result<Guid>.Failure("This doctor does not belong to the selected clinic.");

        var appointment = Appointment.Book(
            request.ClinicId, patientProfile.Id, request.ClinicMembershipId, request.DepartmentId,
            request.ScheduleSlotId, request.AppointmentDate, request.AppointmentTime,
            membership.ConsultationFee ?? 0m, request.Notes);

        await _appointmentRepository.AddAsync(appointment, ct);

        try { slot.ConfirmBooking(); }
        catch (InvalidOperationException ex) { return Result<Guid>.Failure(ex.Message); }

        await _appointmentRepository.SaveChangesAsync(ct);
        await _slotRepository.SaveChangesAsync(ct);

        await _historyRepository.AddAsync(
            AppointmentStatusHistoryEntry.Create(appointment.Id, null, AppointmentStatus.Pending,
                userId, "Appointment booked"), ct);
        await _historyRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(appointment.Id);
    }
}
