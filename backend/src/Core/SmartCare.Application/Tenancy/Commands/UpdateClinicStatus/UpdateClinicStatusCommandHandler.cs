using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.UpdateClinicStatus;

public class UpdateClinicStatusCommandHandler : IRequestHandler<UpdateClinicStatusCommand, Result>
{
    private readonly IClinicRepository _clinicRepository;
    public UpdateClinicStatusCommandHandler(IClinicRepository clinicRepository) => _clinicRepository = clinicRepository;

    public async Task<Result> Handle(UpdateClinicStatusCommand request, CancellationToken ct)
    {
        var clinic = await _clinicRepository.GetByIdAsync(request.ClinicId, ct);
        if (clinic is null) return Result.Failure("Clinic not found.");

        try
        {
            switch (request.Action)
            {
                case ClinicStatusAction.Approve: clinic.Approve(); break;
                case ClinicStatusAction.Suspend: clinic.Suspend(); break;
                case ClinicStatusAction.Reactivate: clinic.Reactivate(); break;
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _clinicRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
