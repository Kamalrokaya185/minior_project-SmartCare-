using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.Domain.Patients;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandHandler : IRequestHandler<CreatePatientProfileCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreatePatientProfileCommandHandler(
        IUserRepository userRepository,
        IPatientProfileRepository patientProfileRepository,
        IUserRoleRepository userRoleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _patientProfileRepository = patientProfileRepository;
        _userRoleRepository = userRoleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(CreatePatientProfileCommand request, CancellationToken ct)
    {
        // 1. Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.email, ct);
        if (existingUser != null)
        {
            return Result<Guid>.Failure("A user with this email already exists.");
        }

        // 2. Hash Password & Create User Entity directly (avoiding nested MediatR calls)
        var hash = _passwordHasher.Hash(request.password);
        var user = User.Register(request.email, hash, request.fullName);

        await _userRepository.AddAsync(user, ct);

        // 3. Create Patient Profile Entity
        var profile = PatientProfile.Create(
            user.Id,
            request.Gender,
            request.DateOfBirth,
            request.NID,
            request.EmergencyContactName,
            request.EmergencyContactRelationship,
            request.EmergencyContactPhone);

        await _patientProfileRepository.AddAsync(profile, ct);

        // 4. Create User Role Entity
        var userRole = UserRole.Create(user.Id, SystemRoles.PatientId, profile.Id);
        await _userRoleRepository.AddAsync(userRole, ct);

        // 5. SAVE EVERYTHING IN A SINGLE TRANSACTION
        // All entities are tracked in the same DbContext session, so SQLite locks once.
        await _patientProfileRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(profile.Id);
    }
}

































//using MediatR;
//using SmartCare.Application.Identity.Commands.RegisterUser;
//using SmartCare.Domain.Identity;
//using SmartCare.Domain.Patients;
//using SmartCare.SharedKernel;

//namespace SmartCare.Application.Patients.Commands.CreatePatientProfile;

//public class CreatePatientProfileCommandHandler : IRequestHandler<CreatePatientProfileCommand, Result<Guid>>
//{
//    private readonly IPatientProfileRepository _patientProfileRepository;
//    private readonly IUserRoleRepository _userRoleRepository;
//    private readonly IMediator _mediator; // 1. Inject IMediator

//    public CreatePatientProfileCommandHandler(
//        IPatientProfileRepository patientProfileRepository,
//        IUserRoleRepository userRoleRepository,
//        IMediator mediator)
//    {
//        _patientProfileRepository = patientProfileRepository;
//        _userRoleRepository = userRoleRepository;
//        _mediator = mediator;
//    }

//    public async Task<Result<Guid>> Handle(CreatePatientProfileCommand request, CancellationToken ct)
//    {
//        // 1. REUSE REGISTER USER COMMAND via MediatR
//        // -----------------------------------------------------------------------------
//        var registerUserCmd = new RegisterUserCommand(
//            request.email,
//            request.password,
//            request.fullName
//        );

//        var userResult = await _mediator.Send(registerUserCmd, ct);

//        //if (!userResult.IsSuccess)
//        //{
//        //    return Result<Guid>.Failure(userResult.Error); // Returns error if email exists or validation fails
//        //}

//        Guid userId = userResult.Value; // Get the created User's ID


//        //var existing = await _patientProfileRepository.GetByUserIdAsync(request.UserId, ct);
//        //if (existing is not null)
//        //    return Result<Guid>.Failure("A patient profile already exists for this user.");

//        var profile = PatientProfile.Create(
//            userId, request.Gender, request.DateOfBirth, request.NID,
//            request.EmergencyContactName, request.EmergencyContactRelationship, request.EmergencyContactPhone);

//        await _patientProfileRepository.AddAsync(profile, ct);
//        await _patientProfileRepository.SaveChangesAsync(ct);   // save so profile.Id is committed first

//        var userRole = UserRole.Create(userId, SystemRoles.PatientId, profile.Id);
//        await _userRoleRepository.AddAsync(userRole, ct);
//        await _userRoleRepository.SaveChangesAsync(ct);

//        return Result<Guid>.Success(profile.Id);
//    }
//}
