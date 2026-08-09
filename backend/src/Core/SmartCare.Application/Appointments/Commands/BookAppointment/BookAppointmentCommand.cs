using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid ClinicId, Guid ClinicMembershipId, Guid? DepartmentId,
    Guid ScheduleSlotId, DateOnly AppointmentDate, TimeOnly AppointmentTime, string? Notes)
    : IRequest<Result<Guid>>;

