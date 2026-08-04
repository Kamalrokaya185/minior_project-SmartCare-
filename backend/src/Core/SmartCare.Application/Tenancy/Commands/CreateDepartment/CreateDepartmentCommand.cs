using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.CreateDepartment;

public record CreateDepartmentCommand(Guid ClinicId, string Name, string? Description) : IRequest<Result<Guid>>;

