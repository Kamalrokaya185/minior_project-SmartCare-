using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.SetClinicMembershipActive;

public class SetClinicMembershipActiveCommandHandler : IRequestHandler<SetClinicMembershipActiveCommand, Result>
{
    private readonly Domain.Tenancy.IClinicMembershipRepository _membershipRepository;
    public SetClinicMembershipActiveCommandHandler(Domain.Tenancy.IClinicMembershipRepository membershipRepository) =>
        _membershipRepository = membershipRepository;

    public async Task<Result> Handle(SetClinicMembershipActiveCommand request, CancellationToken ct)
    {
        var membership = await _membershipRepository.GetByIdAsync(request.ClinicMembershipId, ct);
        if (membership is null) return Result.Failure("Membership not found.");

        if (request.IsActive) membership.Reactivate();
        else membership.Deactivate();

        await _membershipRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
