using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.ExpireAppointment;

public record ExpireAppointmentCommand(Guid AppointmentId) : IRequest<Result>;