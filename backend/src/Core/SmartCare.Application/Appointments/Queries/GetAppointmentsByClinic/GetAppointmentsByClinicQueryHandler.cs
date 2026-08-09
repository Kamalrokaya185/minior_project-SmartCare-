using MediatR;
using SmartCare.Domain.Appointments;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Appointments.Queries.GetAppointmentsByClinic;

public class GetAppointmentsByClinicQueryHandler
    : IRequestHandler<GetAppointmentsByClinicQuery, IReadOnlyList<AppointmentListItemDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IDoctorProfileRepository _doctorProfileRepository;

    public GetAppointmentsByClinicQueryHandler(
        IAppointmentRepository appointmentRepository,
        IPatientProfileRepository patientProfileRepository,
        IUserRepository userRepository,
        IClinicMembershipRepository membershipRepository,
        IDoctorProfileRepository doctorProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientProfileRepository = patientProfileRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _doctorProfileRepository = doctorProfileRepository;
    }

    public async Task<IReadOnlyList<AppointmentListItemDto>> Handle(
        GetAppointmentsByClinicQuery request, CancellationToken ct)
    {
        // Default to today when no date is supplied — matches "by default list is today's"
        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var appointments = await _appointmentRepository.GetByClinicAndDateAsync(request.ClinicId, targetDate, ct);

        var result = new List<AppointmentListItemDto>();
        foreach (var appt in appointments)
        {
            var patientProfile = await _patientProfileRepository.GetByIdAsync(appt.PatientProfileId, ct);
            var patientUser = patientProfile is not null
                ? await _userRepository.GetByIdAsync(patientProfile.UserId, ct) : null;

            var membership = await _membershipRepository.GetByIdAsync(appt.DoctorId, ct);
            var doctor = membership is not null
                ? await _doctorProfileRepository.GetByIdAsync(membership.DoctorId, ct) : null;

            result.Add(new AppointmentListItemDto(
                appt.Id,
                patientUser?.FullName ?? "Unknown patient",
                doctor?.FullName ?? "Unknown doctor",
                doctor?.Specialization,
                appt.AppointmentDate,
                appt.AppointmentTime,
                appt.Status.ToString(),
                appt.PaymentStatus.ToString(),
                appt.FeeAtBooking));
        }

        return result;
    }
}
