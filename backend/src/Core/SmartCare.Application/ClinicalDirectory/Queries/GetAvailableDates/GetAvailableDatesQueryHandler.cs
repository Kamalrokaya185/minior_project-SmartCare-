using MediatR;
using SmartCare.Domain.ClinicalDirectory;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetAvailableDates;

public class GetAvailableDatesQueryHandler : IRequestHandler<GetAvailableDatesQuery, IReadOnlyList<DateOnly>>
{
    private readonly IDoctorScheduleRepository _scheduleRepository;
    public GetAvailableDatesQueryHandler(IDoctorScheduleRepository scheduleRepository) =>
        _scheduleRepository = scheduleRepository;

    public async Task<IReadOnlyList<DateOnly>> Handle(GetAvailableDatesQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var dates = new List<DateOnly>();

        for (int i = 0; i < request.DaysAhead; i++)
        {
            var candidateDate = today.AddDays(i);
            var schedules = await _scheduleRepository.GetApplicableSchedulesAsync(
                request.ClinicMembershipId, candidateDate, ct);

            if (schedules.Any())
                dates.Add(candidateDate);
        }

        return dates;
    }
}
