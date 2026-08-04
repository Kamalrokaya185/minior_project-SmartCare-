using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.RegisterClinic;

public class RegisterClinicCommandHandler : IRequestHandler<RegisterClinicCommand, Result<Guid>>
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterClinicCommandHandler(
        IClinicRepository clinicRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IPasswordHasher passwordHasher)
    {
        _clinicRepository = clinicRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterClinicCommand request, CancellationToken ct)
    {
        var existingSlug = await _clinicRepository.GetBySlugAsync(request.Slug, ct);
        if (existingSlug is not null)
            return Result<Guid>.Failure("A clinic with this slug already exists.");

        var existingUser = await _userRepository.GetByEmailAsync(request.OwnerEmail, ct);
        if (existingUser is not null)
            return Result<Guid>.Failure("An account with this email already exists.");

        // 1. Login account for the clinic owner
        var hash = _passwordHasher.Hash(request.OwnerPassword);
        var user = User.Register(request.OwnerEmail, hash, request.OwnerFullName);
        await _userRepository.AddAsync(user, ct);

        // 2. The clinic itself — Status = Pending until Super Admin approves
        var clinic = Clinic.Register(request.Name, request.Slug, request.Email, request.Phone,
            request.Address, request.City, request.State);
        await _clinicRepository.AddAsync(clinic, ct);
        await _clinicRepository.SaveChangesAsync(ct); // commit so clinic.Id exists for the UserRole below

        // 3. Link the owner's account to the Clinic role + this specific clinic
        var userRole = UserRole.Create(user.Id, SystemRoles.ClinicId, clinic.Id);
        await _userRoleRepository.AddAsync(userRole, ct);
        await _userRoleRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(clinic.Id);
    }
}
