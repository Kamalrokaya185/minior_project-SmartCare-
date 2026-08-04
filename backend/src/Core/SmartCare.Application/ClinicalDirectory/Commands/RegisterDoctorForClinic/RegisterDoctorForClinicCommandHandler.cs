//using MediatR;
//using SmartCare.Application.Common.Interfaces;
//using SmartCare.Domain.ClinicalDirectory;
//using SmartCare.Domain.Identity;
//using SmartCare.Domain.Identity.Repositories;
//using SmartCare.Domain.Tenancy;
//using SmartCare.SharedKernel;

//namespace SmartCare.Application.ClinicalDirectory.Commands.RegisterDoctorForClinic;

//public class RegisterDoctorForClinicCommandHandler
//    : IRequestHandler<RegisterDoctorForClinicCommand, Result<Guid>>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly IDoctorProfileRepository _doctorProfileRepository;
//    private readonly IClinicMembershipRepository _membershipRepository;
//    private readonly IUserRoleRepository _userRoleRepository;
//    private readonly IPasswordHasher _passwordHasher;

//    public RegisterDoctorForClinicCommandHandler(
//        IUserRepository userRepository,
//        IDoctorProfileRepository doctorProfileRepository,
//        IClinicMembershipRepository membershipRepository,
//        IUserRoleRepository userRoleRepository,
//        IPasswordHasher passwordHasher)
//    {
//        _userRepository = userRepository;
//        _doctorProfileRepository = doctorProfileRepository;
//        _membershipRepository = membershipRepository;
//        _userRoleRepository = userRoleRepository;
//        _passwordHasher = passwordHasher;
//    }

//    public async Task<Result<Guid>> Handle(RegisterDoctorForClinicCommand request, CancellationToken ct)
//    {
//        // 1. Reuse existing account if this email already has one, else create it
//        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
//        if (user is null)
//        {
//            var hash = _passwordHasher.Hash(request.TemporaryPassword);
//            user = User.Register(request.Email, hash, request.FullName);
//            await _userRepository.AddAsync(user, ct);
//        }

//        // 2. Create the clinical profile if this person hasn't been a doctor anywhere before
//        var doctorProfile = await _doctorProfileRepository.GetByUserIdAsync(user.Id, ct);
//        if (doctorProfile is null)
//        {
//            var existingLicense = await _doctorProfileRepository.GetByLicenseNumberAsync(request.LicenseNumber, ct);
//            if (existingLicense is not null)
//                return Result<Guid>.Failure("This license number is already registered to another doctor.");

//            doctorProfile = DoctorProfile.Create(user.Id, request.LicenseNumber, request.Qualification,
//                request.Specialization, request.ExperienceYear, request.Biography);
//            await _doctorProfileRepository.AddAsync(doctorProfile, ct);
//            await _doctorProfileRepository.SaveChangesAsync(ct);
//        }

//        // 3. Assign the global "Doctor" UserRole only the first time this person becomes a doctor
//        var existingUserRole = await _userRoleRepository.GetByUserAndRoleAsync(user.Id, SystemRoles.DoctorId, ct);
//        if (existingUserRole is null)
//        {
//            var userRole = UserRole.Create(user.Id, SystemRoles.DoctorId, doctorProfile.Id);
//            await _userRoleRepository.AddAsync(userRole, ct);
//            await _userRoleRepository.SaveChangesAsync(ct);
//        }

//        // 4. Create the clinic membership
//        // ClinicMembership.Create(userId, clinicId, roleId, departmentId, consultationFee)
//        var membership = ClinicMembership.Create(
//            user.Id,
//            request.ClinicId,
//            SystemRoles.DoctorId,
//            request.DepartmentId,
//            request.ConsultationFee);

//        await _membershipRepository.AddAsync(membership, ct);
//        await _membershipRepository.SaveChangesAsync(ct);

//        return Result<Guid>.Success(membership.Id);
//    }
//}