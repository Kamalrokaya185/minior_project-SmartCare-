using MediatR;
using SmartCare.Domain.Patients;

namespace SmartCare.Application.Patients.Queries.GetAllPatients;

public class GetAllPatientsQueryHandler : IRequestHandler<GetAllPatientsQuery, IReadOnlyList<PatientListItemDto>>
{
    private readonly IPatientProfileRepository _patientProfileRepository;
    public GetAllPatientsQueryHandler(IPatientProfileRepository patientProfileRepository) =>
        _patientProfileRepository = patientProfileRepository;

    public async Task<IReadOnlyList<PatientListItemDto>> Handle(GetAllPatientsQuery request, CancellationToken ct)
    {
        var patients = await _patientProfileRepository.GetAllAsync(ct);
        return patients.Select(p => new PatientListItemDto(
            p.Id, p.UserId, p.Gender, p.DateOfBirth, p.NID, p.CreatedAtUtc)).ToList();
    }
}
