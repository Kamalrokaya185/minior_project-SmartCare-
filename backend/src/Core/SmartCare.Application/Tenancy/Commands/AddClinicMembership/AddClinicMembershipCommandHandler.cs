using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.AddClinicMembership;

public class AddClinicMembershipCommandHandler : IRequestHandler<AddClinicMembershipCommand, Result<Guid>>
{
    private readonly IClinicMembershipRepository _membershipRepository;
    public AddClinicMembershipCommandHandler(IClinicMembershipRepository membershipRepository) =>
        _membershipRepository = membershipRepository;

    public async Task<Result<Guid>> Handle(AddClinicMembershipCommand request, CancellationToken ct)
    {
        var alreadyExists = await _membershipRepository.ExistsAsync(request.ClinicId, request.DoctorId, ct);
        if (alreadyExists)
            return Result<Guid>.Failure("This doctor is already assigned to this clinic.");

        var membership = ClinicMembership.Create(
            request.ClinicId, request.DoctorId, request.DepartmentId, request.ConsultationFee);

        await _membershipRepository.AddAsync(membership, ct);
        await _membershipRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(membership.Id);
    }
}