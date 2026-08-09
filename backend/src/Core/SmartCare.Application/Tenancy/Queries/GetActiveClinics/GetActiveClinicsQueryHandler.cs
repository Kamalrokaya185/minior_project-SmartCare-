using MediatR;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Tenancy.Queries.GetActiveClinics;

public class GetActiveClinicsQueryHandler : IRequestHandler<GetActiveClinicsQuery, IReadOnlyList<ActiveClinicDto>>
{
    private readonly IClinicRepository _clinicRepository;
    public GetActiveClinicsQueryHandler(IClinicRepository clinicRepository) => _clinicRepository = clinicRepository;

    public async Task<IReadOnlyList<ActiveClinicDto>> Handle(GetActiveClinicsQuery request, CancellationToken ct)
    {
        var clinics = await _clinicRepository.SearchActiveAsync(request.SearchTerm, ct);
        return clinics.Select(c => new ActiveClinicDto(c.Id, c.Name, c.Slug, c.City, c.State)).ToList();
    }
}
