using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.ReserveSlot;

public class ReserveSlotCommandHandler : IRequestHandler<ReserveSlotCommand, Result<Guid>>
{
    private readonly IScheduleSlotRepository _slotRepository;
    public ReserveSlotCommandHandler(IScheduleSlotRepository slotRepository) => _slotRepository = slotRepository;

    public async Task<Result<Guid>> Handle(ReserveSlotCommand request, CancellationToken ct)
    {
        if (request.SlotDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            return Result<Guid>.Failure("Cannot reserve a slot on a past date.");

        var existing = await _slotRepository.GetByClinicMembershipDateTimeAsync(
            request.ClinicMembershipId, request.SlotDate, request.StartTime, ct);

        if (existing is not null)
        {
            try
            {
                existing.ReReserve(); // only succeeds if it's Cancelled or an expired Reserved hold
            }
            catch (InvalidOperationException)
            {
                return Result<Guid>.Failure("Sorry, this time slot is no longer available. Please pick another.");
            }

            await _slotRepository.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var slot = ScheduleSlot.Reserve(request.ClinicMembershipId, doctorScheduleId: null,
            request.SlotDate, request.StartTime, request.EndTime);

        await _slotRepository.AddAsync(slot, ct);

        try
        {
            await _slotRepository.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<Guid>.Failure("Sorry, this time slot was just taken. Please pick another.");
        }

        return Result<Guid>.Success(slot.Id);
    }
}
