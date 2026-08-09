using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.UpdateDoctorProfileDetails;

public record UpdateDoctorProfileDetailsCommand(Guid DoctorProfileId, string FullName,string Specialization, string? Gender) : IRequest<Result>;
