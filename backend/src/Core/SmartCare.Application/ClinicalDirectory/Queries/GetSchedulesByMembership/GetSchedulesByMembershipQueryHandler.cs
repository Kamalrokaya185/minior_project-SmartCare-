using MediatR;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetSchedulesByMembership;

public class GetSchedulesByMembershipQueryHandler
    : IRequestHandler<GetSchedulesByMembershipQuery, IReadOnlyList<DoctorScheduleDto>>
{
    private readonly Domain.ClinicalDirectory.IDoctorScheduleRepository _scheduleRepository;
    public GetSchedulesByMembershipQueryHandler(Domain.ClinicalDirectory.IDoctorScheduleRepository scheduleRepository) =>
        _scheduleRepository = scheduleRepository;

    public async Task<IReadOnlyList<DoctorScheduleDto>> Handle(GetSchedulesByMembershipQuery request, CancellationToken ct)
    {
        var schedules = await _scheduleRepository.GetAllByMembershipAsync(request.ClinicMembershipId, ct);

        return schedules.Select(s => new DoctorScheduleDto(
            s.Id, s.IsRecurring, s.DayOfWeek, s.SpecificDate,
            s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"), s.SlotDurationMinutes,
            s.EffectiveFrom, s.EffectiveTo, s.IsActive)).ToList();
    }
}
