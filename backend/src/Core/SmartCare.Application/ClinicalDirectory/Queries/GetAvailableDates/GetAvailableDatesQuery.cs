using MediatR;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetAvailableDates;

public record GetAvailableDatesQuery(Guid ClinicMembershipId, int DaysAhead) : IRequest<IReadOnlyList<DateOnly>>;
