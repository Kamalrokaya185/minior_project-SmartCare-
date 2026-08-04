using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, Guid ChangedByUserId, string? Reason) : IRequest<Result>;
