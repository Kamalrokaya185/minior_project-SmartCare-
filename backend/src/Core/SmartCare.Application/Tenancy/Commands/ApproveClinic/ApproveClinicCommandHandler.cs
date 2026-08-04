using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.ApproveClinic;

public class ApproveClinicCommandHandler : IRequestHandler<ApproveClinicCommand, Result>
{
    private readonly IClinicRepository _clinicRepository;
    public ApproveClinicCommandHandler(IClinicRepository clinicRepository) => _clinicRepository = clinicRepository;

    public async Task<Result> Handle(ApproveClinicCommand request, CancellationToken ct)
    {
        var clinic = await _clinicRepository.GetByIdAsync(request.ClinicId, ct);
        if (clinic is null) return Result.Failure("Clinic not found.");

        try
        {
            clinic.Approve();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _clinicRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}