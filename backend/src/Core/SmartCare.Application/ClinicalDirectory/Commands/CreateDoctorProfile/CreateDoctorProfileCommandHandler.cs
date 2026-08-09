using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandHandler : IRequestHandler<CreateDoctorProfileCommand, Result<Guid>>
{
    private readonly IDoctorProfileRepository _doctorProfileRepository;
    public CreateDoctorProfileCommandHandler(IDoctorProfileRepository doctorProfileRepository) =>
        _doctorProfileRepository = doctorProfileRepository;

    public async Task<Result<Guid>> Handle(CreateDoctorProfileCommand request, CancellationToken ct)
    {
       // var existing = await _doctorProfileRepository.GetByLicenseNumberAsync(request.LicenseNumber, ct);
        //if (existing is not null)
        //    return Result<Guid>.Failure("A doctor with this license number is already registered.");

        var profile = DoctorProfile.Create(
           request.FullName, request.LicenseNumber, request.Specialization, request.Gender);
        
        await _doctorProfileRepository.AddAsync(profile, ct);
        await _doctorProfileRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(profile.Id);
    }
}