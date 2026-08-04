using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginCommandHandler(IUserRepository userRepository, IUserRoleRepository userRoleRepository,
        IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<string>.Failure("Invalid email or password.");

        var roles = await _userRoleRepository.GetRoleNamesForUserAsync(user.Id, ct);
        var token = _tokenGenerator.GenerateAccessToken(user, roles.Select(r => r.ToString()));
        return Result<string>.Success(token);
    }
}
