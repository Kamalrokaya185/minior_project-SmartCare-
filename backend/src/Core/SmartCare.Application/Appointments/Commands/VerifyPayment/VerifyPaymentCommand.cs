using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.VerifyPayment;

public record VerifyPaymentCommand(Guid AppointmentId, Guid VerifiedByUserId) : IRequest<Result>;
