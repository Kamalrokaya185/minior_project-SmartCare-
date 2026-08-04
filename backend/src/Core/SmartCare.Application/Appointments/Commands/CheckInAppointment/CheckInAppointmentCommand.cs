using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.CheckInAppointment;

public record CheckInAppointmentCommand(Guid AppointmentId, Guid ChangedByUserId) : IRequest<Result>;
