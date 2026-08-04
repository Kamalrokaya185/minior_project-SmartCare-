using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand(Guid AppointmentId, Guid ChangedByUserId) : IRequest<Result>;
