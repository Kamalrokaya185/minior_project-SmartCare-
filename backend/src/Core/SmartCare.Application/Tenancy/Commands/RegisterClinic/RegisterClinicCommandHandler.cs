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
        // 1. Validate Clinic Slug Uniqueness
        var existingSlug = await _clinicRepository.GetBySlugAsync(request.Slug, ct);
        if (existingSlug is not null)
            return Result<Guid>.Failure("A clinic with this slug already exists.");

        // 2. Validate Owner Email Uniqueness
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            return Result<Guid>.Failure("An account with this email already exists.");

        // 3. Stage User Entity (Owner Account)
        var hash = _passwordHasher.Hash(request.OwnerPassword);
        var user = User.Register(request.Email, hash, request.Name);
        await _userRepository.AddAsync(user, ct);

        // 4. Stage Clinic Entity
        var clinic = Clinic.Register(
            request.Name,
            request.Slug,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.State);
        await _clinicRepository.AddAsync(clinic, ct);

        // 5. Stage UserRole Entity
        var userRole = UserRole.Create(user.Id, SystemRoles.ClinicId, clinic.Id);
        await _userRoleRepository.AddAsync(userRole, ct);

        // 6. SINGLE ATOMIC DATABASE SAVE
        await _clinicRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(clinic.Id);
    }
}