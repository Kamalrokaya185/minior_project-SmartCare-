using MediatR;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Patients;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandHandler : IRequestHandler<CreatePatientProfileCommand, Result<Guid>>
{
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public CreatePatientProfileCommandHandler(
        IPatientProfileRepository patientProfileRepository,
        IUserRoleRepository userRoleRepository)
    {
        _patientProfileRepository = patientProfileRepository;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePatientProfileCommand request, CancellationToken ct)
    {
        var existing = await _patientProfileRepository.GetByUserIdAsync(request.UserId, ct);
        if (existing is not null)
            return Result<Guid>.Failure("A patient profile already exists for this user.");

        var profile = PatientProfile.Create(
            request.UserId, request.Gender, request.DateOfBirth, request.NID,
            request.EmergencyContactName, request.EmergencyContactRelationship, request.EmergencyContactPhone);

        await _patientProfileRepository.AddAsync(profile, ct);
        await _patientProfileRepository.SaveChangesAsync(ct);   // save so profile.Id is committed first

        var userRole = UserRole.Create(request.UserId, SystemRoles.PatientId, profile.Id);
        await _userRoleRepository.AddAsync(userRole, ct);
        await _userRoleRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(profile.Id);
    }
}
