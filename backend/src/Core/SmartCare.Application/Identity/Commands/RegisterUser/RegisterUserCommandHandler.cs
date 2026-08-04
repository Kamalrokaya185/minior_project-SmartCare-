using System;
using System.Collections.Generic;
using System.Text;

using MediatR;
using SmartCare.Application.Common.Interfaces;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return Result<Guid>.Failure("An account with this email already exists.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Register(request.Email, hash, request.FullName);

        await _userRepository.AddAsync(user, ct);
        return Result<Guid>.Success(user.Id);
    }
}
