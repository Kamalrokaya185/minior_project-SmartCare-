using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetDoctorAvailability;

public record TimeSlotDto(string StartTime, string EndTime, bool IsAvailable);

public record GetDoctorAvailabilityQuery(Guid ClinicMembershipId, DateOnly Date)
    : IRequest<Result<IReadOnlyList<TimeSlotDto>>>;