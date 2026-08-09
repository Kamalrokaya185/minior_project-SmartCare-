using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Tenancy.Queries.GetDoctorsByClinic;

public class GetDoctorsByClinicQueryHandler : IRequestHandler<GetDoctorsByClinicQuery, IReadOnlyList<DoctorListItemDto>>
{
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IDoctorProfileRepository _doctorProfileRepository;

    public GetDoctorsByClinicQueryHandler(
        IClinicMembershipRepository membershipRepository, IDoctorProfileRepository doctorProfileRepository)
    {
        _membershipRepository = membershipRepository;
        _doctorProfileRepository = doctorProfileRepository;
    }

    public async Task<IReadOnlyList<DoctorListItemDto>> Handle(GetDoctorsByClinicQuery request, CancellationToken ct)
    {
        var memberships = await _membershipRepository.GetByClinicAndDepartmentAsync(
            request.ClinicId, request.DepartmentId, request.ActiveOnly, ct);

        var result = new List<DoctorListItemDto>();
        foreach (var membership in memberships)
        {
            var doctor = await _doctorProfileRepository.GetByIdAsync(membership.DoctorId, ct);
            if (doctor is null) continue;

            result.Add(new DoctorListItemDto(
                membership.Id, doctor.Id, doctor.FullName, doctor.Specialization,
                 membership.ConsultationFee, membership.DepartmentId, membership.IsActive, doctor.LicenseNumber, doctor.Gender));
        }
        return result;
    }
}
