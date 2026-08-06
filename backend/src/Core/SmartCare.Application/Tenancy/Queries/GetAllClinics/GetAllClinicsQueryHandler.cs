using MediatR;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Tenancy.Queries.GetAllClinics;

public class GetAllClinicsQueryHandler : IRequestHandler<GetAllClinicsQuery, IReadOnlyList<ClinicListItemDto>>
{
    private readonly IClinicRepository _clinicRepository;
    public GetAllClinicsQueryHandler(IClinicRepository clinicRepository) => _clinicRepository = clinicRepository;

    public async Task<IReadOnlyList<ClinicListItemDto>> Handle(GetAllClinicsQuery request, CancellationToken ct)
    {
        var clinics = await _clinicRepository.GetAllAsync(ct);
        return clinics.Select(c => new ClinicListItemDto(
            c.Id, c.Name, c.Slug, c.Email, c.Phone, c.City, c.State,
            c.Status, c.CreatedAtUtc, c.ApprovedAtUtc)).ToList();
    }
}
