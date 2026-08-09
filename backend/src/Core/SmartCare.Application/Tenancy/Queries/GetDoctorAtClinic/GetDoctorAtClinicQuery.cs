using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Queries.GetDoctorAtClinic;

public record DoctorAtClinicDto(
    Guid ClinicMembershipId, Guid DoctorProfileId, string FullName, string LicenseNumber,
    string Specialization, string? Gender, Guid? DepartmentId, decimal? ConsultationFee, bool IsActive);

public record GetDoctorAtClinicQuery(Guid ClinicId, Guid ClinicMembershipId) : IRequest<Result<DoctorAtClinicDto>>;
