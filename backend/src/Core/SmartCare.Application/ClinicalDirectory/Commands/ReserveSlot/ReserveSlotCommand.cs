using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.ClinicalDirectory.Commands.ReserveSlot;

public record ReserveSlotCommand(Guid ClinicMembershipId, DateOnly SlotDate, TimeOnly StartTime, TimeOnly EndTime)
    : IRequest<Result<Guid>>;

