using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Appointments.Commands.SubmitPaymentProof;

public record SubmitPaymentProofCommand(Guid AppointmentId, string PaymentProofUrl, string PaymentMethod)
    : IRequest<Result>;

