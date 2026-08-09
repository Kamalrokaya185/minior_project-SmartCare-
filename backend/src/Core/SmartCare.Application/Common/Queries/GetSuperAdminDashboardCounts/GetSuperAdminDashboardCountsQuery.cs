using MediatR;

namespace SmartCare.Application.Common.Queries.GetSuperAdminDashboardCounts;

public record DashboardCountsDto(
    int TotalClinics, int ActiveClinics, int PendingClinics, int SuspendedClinics,
    int TotalPatients, int ActivePatients, int InactivePatients);

public record GetSuperAdminDashboardCountsQuery : IRequest<DashboardCountsDto>;
