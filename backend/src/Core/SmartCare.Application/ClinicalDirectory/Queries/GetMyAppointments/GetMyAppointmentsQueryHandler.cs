using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Appointments.Queries.GetMyAppointments;

public class GetMyAppointmentsQueryHandler : IRequestHandler<GetMyAppointmentsQuery, IReadOnlyList<MyAppointmentDto>>
{
    private readonly Domain.Appointments.IAppointmentRepository _appointmentRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly IClinicRepository _clinicRepository;
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IDoctorProfileRepository _doctorProfileRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAppointmentsQueryHandler(
        Domain.Appointments.IAppointmentRepository appointmentRepository,
        IPatientProfileRepository patientProfileRepository,
        IClinicRepository clinicRepository,
        IClinicMembershipRepository membershipRepository,
        IDoctorProfileRepository doctorProfileRepository,
        IDepartmentRepository departmentRepository,
        ICurrentUserService currentUserService)
    {
        _appointmentRepository = appointmentRepository;
        _patientProfileRepository = patientProfileRepository;
        _clinicRepository = clinicRepository;
        _membershipRepository = membershipRepository;
        _doctorProfileRepository = doctorProfileRepository;
        _departmentRepository = departmentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<MyAppointmentDto>> Handle(GetMyAppointmentsQuery request, CancellationToken ct)
    {
        if (_currentUserService.UserId is not Guid userId) return Array.Empty<MyAppointmentDto>();

        var patientProfile = await _patientProfileRepository.GetByUserIdAsync(userId, ct);
        if (patientProfile is null) return Array.Empty<MyAppointmentDto>();

        var appointments = await _appointmentRepository.GetByPatientAsync(patientProfile.Id, ct);

        var result = new List<MyAppointmentDto>();
        foreach (var appt in appointments)
        {
            var clinic = await _clinicRepository.GetByIdAsync(appt.ClinicId, ct);
            var membership = await _membershipRepository.GetByIdAsync(appt.DoctorId, ct);
            var doctor = membership is not null ? await _doctorProfileRepository.GetByIdAsync(membership.DoctorId, ct) : null;
            var department = appt.DepartmentId is Guid deptId ? await _departmentRepository.GetByIdAsync(deptId, ct) : null;

            result.Add(new MyAppointmentDto(
                appt.Id, clinic?.Name ?? "Unknown clinic", doctor?.FullName ?? "Unknown doctor",
                doctor?.Specialization, department?.Name, appt.AppointmentDate, appt.AppointmentTime,
                appt.Status.ToString(), appt.PaymentStatus.ToString(), appt.FeeAtBooking));
        }
        return result;
    }
}
