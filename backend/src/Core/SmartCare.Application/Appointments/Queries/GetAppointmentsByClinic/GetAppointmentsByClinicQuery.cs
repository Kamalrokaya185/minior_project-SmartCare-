using MediatR;

namespace SmartCare.Application.Appointments.Queries.GetAppointmentsByClinic;

public record AppointmentListItemDto(
    Guid AppointmentId,
    string PatientName,
    string DoctorName,
    string? Specialization,
    DateOnly AppointmentDate,
    TimeOnly AppointmentTime,
    string Status,
    string PaymentStatus,
    decimal FeeAtBooking);

public record GetAppointmentsByClinicQuery(Guid ClinicId, DateOnly? Date)
    : IRequest<IReadOnlyList<AppointmentListItemDto>>;
