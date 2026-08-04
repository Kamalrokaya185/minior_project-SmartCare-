using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.ApproveClinic;

public record ApproveClinicCommand(Guid ClinicId) : IRequest<Result>;
