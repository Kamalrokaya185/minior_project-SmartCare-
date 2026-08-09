using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.UpdateDoctorProfileDetails;

public class UpdateDoctorProfileDetailsCommandHandler : IRequestHandler<UpdateDoctorProfileDetailsCommand, Result>
{
    private readonly IDoctorProfileRepository _doctorProfileRepository;
    public UpdateDoctorProfileDetailsCommandHandler(IDoctorProfileRepository doctorProfileRepository) =>
        _doctorProfileRepository = doctorProfileRepository;

    public async Task<Result> Handle(UpdateDoctorProfileDetailsCommand request, CancellationToken ct)
    {
        var doctor = await _doctorProfileRepository.GetByIdAsync(request.DoctorProfileId, ct);
        if (doctor is null) return Result.Failure("Doctor profile not found.");

        try
        {
            doctor.UpdateDetails(request.FullName,request.Specialization, request.Gender);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _doctorProfileRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
