using MediatR;

namespace SmartCare.Application.Tenancy.Queries.GetDoctorsByClinic;

public record DoctorListItemDto(
    Guid ClinicMembershipId, Guid DoctorProfileId, string FullName,
    string? Specialization, decimal? ConsultationFee, Guid? DepartmentId, bool IsActive, string? LicenseNumber, string? Gender);

public record GetDoctorsByClinicQuery(Guid ClinicId, Guid? DepartmentId, bool ActiveOnly) : IRequest<IReadOnlyList<DoctorListItemDto>>;
