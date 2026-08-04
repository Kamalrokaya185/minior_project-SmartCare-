using MediatR;

namespace SmartCare.Application.Tenancy.Queries.GetDoctorsByClinic;

public record DoctorListItemDto(
    Guid ClinicMembershipId, Guid DoctorProfileId, string FullName,
    string? Specialization, string? PhotoUrl, decimal? ConsultationFee, Guid? DepartmentId);

public record GetDoctorsByClinicQuery(Guid ClinicId, Guid? DepartmentId) : IRequest<IReadOnlyList<DoctorListItemDto>>;
