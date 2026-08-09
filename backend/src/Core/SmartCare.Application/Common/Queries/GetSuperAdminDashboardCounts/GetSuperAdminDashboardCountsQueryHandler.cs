using MediatR;
using SmartCare.Domain.Patients;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Common.Queries.GetSuperAdminDashboardCounts;

public class GetSuperAdminDashboardCountsQueryHandler
    : IRequestHandler<GetSuperAdminDashboardCountsQuery, DashboardCountsDto>
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;

    public GetSuperAdminDashboardCountsQueryHandler(
        IClinicRepository clinicRepository, IPatientProfileRepository patientProfileRepository)
    {
        _clinicRepository = clinicRepository;
        _patientProfileRepository = patientProfileRepository;
    }

    public async Task<DashboardCountsDto> Handle(GetSuperAdminDashboardCountsQuery request, CancellationToken ct)
    {
        var clinicCounts = await _clinicRepository.GetStatusCountsAsync(ct);
        var patientCounts = await _patientProfileRepository.GetStatusCountsAsync(ct);

        return new DashboardCountsDto(
            clinicCounts.Total, clinicCounts.Active, clinicCounts.Pending, clinicCounts.Suspended,
            patientCounts.Total, patientCounts.Active, patientCounts.Inactive);
    }
}
