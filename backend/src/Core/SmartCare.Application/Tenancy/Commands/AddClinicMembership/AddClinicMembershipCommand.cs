using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.AddClinicMembership;

public record AddClinicMembershipCommand(
    Guid ClinicId, Guid DoctorId, Guid? DepartmentId, decimal? ConsultationFee) : IRequest<Result<Guid>>;