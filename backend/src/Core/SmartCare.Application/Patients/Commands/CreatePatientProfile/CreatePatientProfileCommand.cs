using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Patients.Commands.CreatePatientProfile;

public record CreatePatientProfileCommand(
    string email,
    string password,
    string fullName,
    string? Gender, DateOnly? DateOfBirth, string? NID,
    string? EmergencyContactName, string? EmergencyContactRelationship, string? EmergencyContactPhone)
    : IRequest<Result<Guid>>;
