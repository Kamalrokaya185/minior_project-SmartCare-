using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityQueryHandler
    : IRequestHandler<GetDoctorAvailabilityQuery, Result<IReadOnlyList<TimeSlotDto>>>
{
    private readonly IDoctorScheduleRepository _scheduleRepository;
    private readonly IScheduleSlotRepository _slotRepository;

    public GetDoctorAvailabilityQueryHandler(
        IDoctorScheduleRepository scheduleRepository, IScheduleSlotRepository slotRepository)
    {
        _scheduleRepository = scheduleRepository;
        _slotRepository = slotRepository;
    }

    public async Task<Result<IReadOnlyList<TimeSlotDto>>> Handle(GetDoctorAvailabilityQuery request, CancellationToken ct)
    {
        if (request.Date < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            return Result<IReadOnlyList<TimeSlotDto>>.Failure("Cannot check availability for a past date.");

        var schedules = await _scheduleRepository.GetApplicableSchedulesAsync(request.ClinicMembershipId, request.Date, ct);
        var takenTimes = await _slotRepository.GetTakenStartTimesAsync(request.ClinicMembershipId, request.Date, ct);

        var availability = SlotAvailabilityService.BuildAvailability(schedules, takenTimes);

        var dtos = availability
            .Select(a => new TimeSlotDto(a.StartTime.ToString("HH:mm"), a.EndTime.ToString("HH:mm"), a.IsAvailable))
            .ToList();

        return Result<IReadOnlyList<TimeSlotDto>>.Success(dtos);
    }
}
