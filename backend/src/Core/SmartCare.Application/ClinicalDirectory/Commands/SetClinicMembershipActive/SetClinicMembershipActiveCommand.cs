using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.SetClinicMembershipActive;

public record SetClinicMembershipActiveCommand(Guid ClinicMembershipId, bool IsActive) : IRequest<Result>;
