using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.SetDoctorScheduleActive;

public class SetDoctorScheduleActiveCommandHandler : IRequestHandler<SetDoctorScheduleActiveCommand, Result>
{
    private readonly IDoctorScheduleRepository _scheduleRepository;
    public SetDoctorScheduleActiveCommandHandler(IDoctorScheduleRepository scheduleRepository) =>
        _scheduleRepository = scheduleRepository;

    public async Task<Result> Handle(SetDoctorScheduleActiveCommand request, CancellationToken ct)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, ct);
        if (schedule is null) return Result.Failure("Schedule not found.");

        if (request.IsActive) schedule.Reactivate();
        else schedule.Deactivate();

        await _scheduleRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
