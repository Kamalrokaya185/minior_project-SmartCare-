using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.RejectPayment;

public record RejectPaymentCommand(Guid AppointmentId, Guid RejectedByUserId, string? Reason) : IRequest<Result>;
