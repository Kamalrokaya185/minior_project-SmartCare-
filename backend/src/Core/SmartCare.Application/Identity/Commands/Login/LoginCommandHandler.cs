using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ISuperAdminCredentialsProvider _superAdminProvider;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        ISuperAdminCredentialsProvider superAdminProvider)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _superAdminProvider = superAdminProvider;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        // SuperAdmin check happens first, entirely outside the database.
        if (_superAdminProvider.Validate(request.Email, request.Password))
        {
            var adminRoles = new List<string> { "SuperAdmin" };
            var adminToken = _tokenGenerator.GenerateAccessToken(
                _superAdminProvider.SuperAdminUserId, request.Email, adminRoles);

            return Result<LoginResponse>.Success(
                new LoginResponse(adminToken, adminRoles, _superAdminProvider.SuperAdminUserId, "Super Admin", _superAdminProvider.SuperAdminUserId));
        }

        // Not the admin — proceed with the normal Users/UserRoles-backed flow.
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid email or password.");

        var roles = await _userRoleRepository.GetRoleNamesForUserAsync(user.Id, ct);
        //var profileIds = await _userRoleRepository.GetProfileIdNamesForUserAsync(user.Id, ct);
        //var profileid = profileIds.FirstOrDefault();
        var profileid = await _userRoleRepository.GetProfileIdNamesForUserAsync(user.Id, ct) ?? Guid.Empty;

        var roleStrings = roles.Select(r => r.ToString()).ToList();
        var token = _tokenGenerator.GenerateAccessToken(user.Id, user.Email, roleStrings);

        return Result<LoginResponse>.Success(new LoginResponse(token, roleStrings, user.Id, user.FullName, profileid));
    }
}
