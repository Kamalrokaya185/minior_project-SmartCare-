using MediatR;
using SmartCare.Domain.Identity;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FullName)
    : IRequest<Result<Guid>>;
