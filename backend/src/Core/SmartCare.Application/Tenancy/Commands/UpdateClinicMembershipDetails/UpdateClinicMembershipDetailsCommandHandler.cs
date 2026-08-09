using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.UpdateClinicMembershipDetails;

public class UpdateClinicMembershipDetailsCommandHandler : IRequestHandler<UpdateClinicMembershipDetailsCommand, Result>
{
    private readonly IClinicMembershipRepository _membershipRepository;
    public UpdateClinicMembershipDetailsCommandHandler(IClinicMembershipRepository membershipRepository) =>
        _membershipRepository = membershipRepository;

    public async Task<Result> Handle(UpdateClinicMembershipDetailsCommand request, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByIdAsync(request.ClinicMembershipId, ct);
        if (membership is null) return Result.Failure("Membership not found.");

        try
        {
            membership.UpdateDetails( request.DepartmentId, request.ConsultationFee);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _membershipRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
