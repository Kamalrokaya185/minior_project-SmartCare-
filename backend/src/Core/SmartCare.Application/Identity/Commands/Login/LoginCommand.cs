using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.Login;

public record LoginResponse(string Token, IReadOnlyList<string> Roles, Guid UserId, string FullName ,Guid Profileid);

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
