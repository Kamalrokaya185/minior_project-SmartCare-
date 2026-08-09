using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.SetDoctorScheduleActive;

public record SetDoctorScheduleActiveCommand(Guid ScheduleId, bool IsActive) : IRequest<Result>;
