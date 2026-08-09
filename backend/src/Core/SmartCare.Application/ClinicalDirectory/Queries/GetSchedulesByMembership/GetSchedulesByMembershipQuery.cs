using MediatR;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetSchedulesByMembership;

public record DoctorScheduleDto(
    Guid Id, bool IsRecurring, int? DayOfWeek, DateOnly? SpecificDate,
    string StartTime, string EndTime, int SlotDurationMinutes,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive);

public record GetSchedulesByMembershipQuery(Guid ClinicMembershipId) : IRequest<IReadOnlyList<DoctorScheduleDto>>;
