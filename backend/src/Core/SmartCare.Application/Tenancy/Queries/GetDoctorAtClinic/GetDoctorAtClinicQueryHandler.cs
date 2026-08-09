using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Queries.GetDoctorAtClinic;

public class GetDoctorAtClinicQueryHandler : IRequestHandler<GetDoctorAtClinicQuery, Result<DoctorAtClinicDto>>
{
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IDoctorProfileRepository _doctorProfileRepository;

    public GetDoctorAtClinicQueryHandler(
        IClinicMembershipRepository membershipRepository, IDoctorProfileRepository doctorProfileRepository)
    {
        _membershipRepository = membershipRepository;
        _doctorProfileRepository = doctorProfileRepository;
    }

    public async Task<Result<DoctorAtClinicDto>> Handle(GetDoctorAtClinicQuery request, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByIdAsync(request.ClinicMembershipId, ct);
        if (membership is null || membership.ClinicId != request.ClinicId)
            return Result<DoctorAtClinicDto>.Failure("Doctor membership not found at this clinic.");

        var doctor = await _doctorProfileRepository.GetByIdAsync(membership.DoctorId, ct);
        if (doctor is null) return Result<DoctorAtClinicDto>.Failure("Doctor profile not found.");

        return Result<DoctorAtClinicDto>.Success(new DoctorAtClinicDto(
            membership.Id, doctor.Id, doctor.FullName, doctor.LicenseNumber, doctor.Specialization,
            doctor.Gender, membership.DepartmentId, membership.ConsultationFee, membership.IsActive));
    }
}
