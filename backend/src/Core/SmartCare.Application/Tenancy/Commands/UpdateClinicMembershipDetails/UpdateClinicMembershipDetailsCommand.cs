using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.UpdateClinicMembershipDetails;

public record UpdateClinicMembershipDetailsCommand(
    Guid ClinicMembershipId, Guid? DepartmentId, decimal? ConsultationFee) : IRequest<Result>;
