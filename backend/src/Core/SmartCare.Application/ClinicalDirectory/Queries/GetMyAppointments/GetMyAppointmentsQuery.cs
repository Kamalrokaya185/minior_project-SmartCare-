using MediatR;

namespace SmartCare.Application.Appointments.Queries.GetMyAppointments;

public record MyAppointmentDto(
    Guid AppointmentId, string ClinicName, string DoctorName, string? Specialization,
    string? DepartmentName, DateOnly AppointmentDate, TimeOnly AppointmentTime,
    string Status, string PaymentStatus, decimal FeeAtBooking);

public record GetMyAppointmentsQuery : IRequest<IReadOnlyList<MyAppointmentDto>>;
