using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.CreateDoctorProfile;

public record CreateDoctorProfileCommand(
    string FullName, string LicenseNumber, string? Qualification, string? Specialization, int? ExperienceYear,
    string? Gender, string? Phone, string? Email, string? PhotoUrl, string? Biography)
    : IRequest<Result<Guid>>;
