using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.CreateDoctorSchedule;

public class CreateDoctorScheduleCommandHandler : IRequestHandler<CreateDoctorScheduleCommand, Result<Guid>>
{
    private readonly IDoctorScheduleRepository _scheduleRepository;
    public CreateDoctorScheduleCommandHandler(IDoctorScheduleRepository scheduleRepository) =>
        _scheduleRepository = scheduleRepository;

    public async Task<Result<Guid>> Handle(CreateDoctorScheduleCommand request, CancellationToken ct)
    {
        try
        {
            var schedule = request.IsRecurring
                ? DoctorSchedule.CreateRecurring(request.ClinicMembershipId, request.DayOfWeek!.Value,
                    request.StartTime, request.EndTime, request.SlotDurationMinutes,
                    request.EffectiveFrom, request.EffectiveTo)
                : DoctorSchedule.CreateOneTime(request.ClinicMembershipId, request.SpecificDate!.Value,
                    request.StartTime, request.EndTime, request.SlotDurationMinutes);

            await _scheduleRepository.AddAsync(schedule, ct);
            await _scheduleRepository.SaveChangesAsync(ct);

            return Result<Guid>.Success(schedule.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}