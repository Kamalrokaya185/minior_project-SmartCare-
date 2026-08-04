using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.RegisterDoctorForClinic;

public record RegisterDoctorForClinicCommand(
    Guid ClinicId,
    string Email,
    string TemporaryPassword,
    string FullName,
    string LicenseNumber,
    string? Qualification,
    string? Specialization,
    int? ExperienceYear,
    string? Biography,
    Guid? DepartmentId,
    decimal? ConsultationFee) : IRequest<Result<Guid>>;
