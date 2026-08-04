using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.CreateDoctorSchedule;

public record CreateDoctorScheduleCommand(
    Guid ClinicMembershipId,
    bool IsRecurring,
    int? DayOfWeek,          // required if IsRecurring
    DateOnly? SpecificDate,  // required if !IsRecurring
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo) : IRequest<Result<Guid>>;
