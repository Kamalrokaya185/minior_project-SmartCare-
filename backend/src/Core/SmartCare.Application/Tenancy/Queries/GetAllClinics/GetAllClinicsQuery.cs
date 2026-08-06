using MediatR;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Tenancy.Queries.GetAllClinics;

public record ClinicListItemDto(
    Guid Id, string Name, string Slug, string? Email, string? Phone,
    string? City, string? State, ClinicStatus Status, DateTime CreatedAtUtc, DateTime? ApprovedAtUtc);

public record GetAllClinicsQuery : IRequest<IReadOnlyList<ClinicListItemDto>>;
