using MediatR;

namespace SmartCare.Application.Patients.Queries.GetAllPatients;

public record PatientListItemDto(
    Guid Id, Guid UserId, string? Gender, DateOnly? DateOfBirth, string? NID, DateTime CreatedAtUtc);

public record GetAllPatientsQuery : IRequest<IReadOnlyList<PatientListItemDto>>;
