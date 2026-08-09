using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.RegisterDoctorAtClinic;

public record RegisterDoctorAtClinicCommand(
    Guid ClinicId, string FullName, string LicenseNumber, string Specialization, string? Gender,
    Guid? DepartmentId, decimal? ConsultationFee,

    // Optional initial schedule — all null/omitted means "set up schedule later"
    bool? IsRecurring, int? DayOfWeek, DateOnly? SpecificDate,
    TimeOnly? StartTime, TimeOnly? EndTime, int? SlotDurationMinutes,
    DateOnly? EffectiveFrom, DateOnly? EffectiveTo) : IRequest<Result<Guid>>; // returns ClinicMembership.Id

