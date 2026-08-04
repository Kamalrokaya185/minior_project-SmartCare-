using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.RegisterClinic;

public record RegisterClinicCommand(
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFullName,
    string Name, string Slug, string? Email, string? Phone,
    string? Address, string? City, string? State) : IRequest<Result<Guid>>;