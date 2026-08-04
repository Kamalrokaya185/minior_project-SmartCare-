using MediatR;

namespace SmartCare.Application.Tenancy.Queries.GetDepartmentsByClinic;

public record DepartmentDto(Guid Id, string Name, string? Description);

public record GetDepartmentsByClinicQuery(Guid ClinicId) : IRequest<IReadOnlyList<DepartmentDto>>;
