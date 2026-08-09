using MediatR;

namespace SmartCare.Application.Tenancy.Queries.GetActiveClinics;

public record ActiveClinicDto(Guid Id, string Name, string Slug, string? City, string? State);

public record GetActiveClinicsQuery(string? SearchTerm) : IRequest<IReadOnlyList<ActiveClinicDto>>;
