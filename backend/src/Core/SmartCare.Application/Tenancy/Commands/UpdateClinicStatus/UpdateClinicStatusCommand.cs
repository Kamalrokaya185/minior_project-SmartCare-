using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.UpdateClinicStatus;

public enum ClinicStatusAction { Approve, Suspend, Reactivate }

public record UpdateClinicStatusCommand(Guid ClinicId, ClinicStatusAction Action) : IRequest<Result>;
