using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, string? Reason) : IRequest<Result>;
